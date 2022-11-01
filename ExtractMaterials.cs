using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Autodesk.Revit.DB;
using Autodesk.Revit.Attributes;
using DesignAutomationFramework;
using Autodesk.Revit.ApplicationServices;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace ExtractMaterialQuantities
{
	[Regeneration(RegenerationOption.Manual)]
	[Transaction(TransactionMode.Manual)]
	public class ExtractMaterials : IExternalDBApplication
	{
			public static void ExtractModelMaterials(DesignAutomationData data)
			{

				if (data == null) throw new ArgumentNullException(nameof(data));

				Application rvtApp = data.RevitApp;
				if (rvtApp == null) throw new InvalidDataException(nameof(rvtApp));

				string modelPath = data.FilePath;
				if (String.IsNullOrWhiteSpace(modelPath)) throw new InvalidDataException(nameof(modelPath));

				Document doc = data.RevitDoc;
				if (doc == null) throw new InvalidOperationException("Could not open document.");

				dynamic urnResult = new JObject();

				List<Element> elements = new FilteredElementCollector(doc).WhereElementIsNotElementType().ToList();

				foreach (Element element in elements)
				{
					try
					{
						Category elementCategory = element.Category;

						List<ElementId> materialsIds = element.GetMaterialIds(false).ToList();

						foreach (ElementId materialId in materialsIds)
						{
							try
							{
								Element material = doc.GetElement(materialId);
								double materialArea = element.GetMaterialArea(materialId, false);
								double materialVolume = element.GetMaterialVolume(materialId);

								dynamic newMaterial = new JObject();
								newMaterial.externalId = element.UniqueId.ToString();
								newMaterial.revitcategory = elementCategory.Name;
								newMaterial.revitmaterial = material.Name;
								newMaterial.materialareaqty = UnitUtils.ConvertFromInternalUnits(materialArea, UnitTypeId.SquareMeters);
								newMaterial.materialareaqtytype = UnitTypeId.SquareMeters.TypeId;
								newMaterial.materialvolumeqty = UnitUtils.ConvertFromInternalUnits(materialVolume, UnitTypeId.CubicMeters);
								newMaterial.materialvolumeqtytype = UnitTypeId.CubicMeters.TypeId;

								Parameter elementLength = element.LookupParameter("Length");
								newMaterial.elementlength = UnitUtils.ConvertFromInternalUnits(elementLength != null ? elementLength.AsDouble() : 0, UnitTypeId.Meters);
								newMaterial.elementlengthqtytype = UnitTypeId.Meters.TypeId;

								urnResult.results.Add(newMaterial);
							}
							catch (Exception ex)
							{
								Console.WriteLine($"Error with material {materialId} from element {element.UniqueId}!");
								Console.WriteLine(ex.Message);
							}
						}
					}
					catch (Exception ex)
					{
						Console.WriteLine($"Error with element {element.UniqueId}!");
						Console.WriteLine(ex.Message);
					}

				}

				// save all to a .json file
				using (StreamWriter file = File.CreateText("result.json"))
				using (JsonTextWriter writer = new JsonTextWriter(file))
				{
					urnResult.WriteTo(writer);
				}

			}

			public ExternalDBApplicationResult OnShutdown(ControlledApplication application)
			{
				return ExternalDBApplicationResult.Succeeded;
			}

			public ExternalDBApplicationResult OnStartup(ControlledApplication application)
			{
				DesignAutomationBridge.DesignAutomationReadyEvent += HandleDesignAutomationReadyEvent;
				return ExternalDBApplicationResult.Succeeded;
			}

			private void HandleDesignAutomationReadyEvent(object sender, DesignAutomationReadyEventArgs e)
			{
				e.Succeeded = true;
				ExtractModelMaterials(e.DesignAutomationData);
			}
	}
}
