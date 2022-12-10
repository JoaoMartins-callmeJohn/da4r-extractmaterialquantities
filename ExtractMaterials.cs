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
using System.Threading.Tasks;
using System.Net.Http;
using System.Text;

namespace ExtractMaterialQuantities
{
	[Regeneration(RegenerationOption.Manual)]
	[Transaction(TransactionMode.Manual)]
	public class ExtractMaterials : IExternalDBApplication
	{
			public static void ExtractModelMaterials(DesignAutomationData data)
			{
				InputParams inputParameters = JsonConvert.DeserializeObject<InputParams>(File.ReadAllText("params.json"));

				if (data == null) throw new ArgumentNullException(nameof(data));

				Application rvtApp = data.RevitApp;
				if (rvtApp == null) throw new InvalidDataException(nameof(rvtApp));

				string modelPath = data.FilePath;
				if (String.IsNullOrWhiteSpace(modelPath)) throw new InvalidDataException(nameof(modelPath));

				Document doc = data.RevitDoc;
				if (doc == null) throw new InvalidOperationException("Could not open document.");

				dynamic urnResult = new JObject();
				urnResult.results = new JArray();

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
								newMaterial.externalId = element.UniqueId;
								newMaterial.revitcategory = elementCategory.Name;
								newMaterial.revitmaterial = material.Name;
								newMaterial.materialareaqty = materialArea;
								newMaterial.materialvolumeqty = materialVolume;

								urnResult.results.Add(newMaterial);

								SendMaterial(inputParameters.url, newMaterial);
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

		private static async Task SendMaterial(string url, JObject newMaterial)
		{
			var client = new HttpClient();
			var content = new StringContent(newMaterial.ToString(), Encoding.UTF8, "application/json");
			var result = client.PostAsync(url, content).Result;
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
	public class InputParams
	{
		public string url { get; set; }

	}
}
