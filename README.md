# Revit Materials Extractor bundle for Forge Design Automation

[![Design Automation](https://img.shields.io/badge/Design%20Automation-v3-green.svg)](http://developer.autodesk.com/)

![Windows](https://img.shields.io/badge/Plugins-Windows-lightgrey.svg)
![.NET](https://img.shields.io/badge/.NET%20Framework-4.8-blue.svg)
[![Revit-2022](https://img.shields.io/badge/Revit-2022-lightgrey.svg)](http://autodesk.com/revit)


# Description

This sample demonstrates a way to retrieve quantities of materials for a Revit project

# Development Setup

## Prerequisites

1. **Forge Account**: Learn how to create a Forge Account, activate subscription and create an app at [this tutorial](http://learnforge.autodesk.io/#/account/). 
2. **Visual Studio 2022** (Windows).
3. **Revit 2022**: required to compile changes into the plugin

## Design Automation Setup

### Activity example

```json
{
  "id": "RevitMaterialsExtractorActivity",
  "commandLine": [
    "$(engine.path)\\\\revitcoreconsole.exe /i \"$(args[inputFile].path)\" /al \"$(appbundles[MaterialsExtractor].path)\""
  ],
  "parameters": {
    "inputFile": {
      "verb": "get",
      "description": "Input Revit File",
      "required": true,
      "localName": "$(inputFile)"
    },
    "result": {
      "zip": false,
      "verb": "put",
      "description": "Extracted materials from model",
      "localName": "result.json"
    }
  },
  "engine": "Autodesk.Revit+2022",
  "appbundles": [
    "Autodesk.MaterialsExtractor+dev"
  ],
  "description": "Activity of Revit Materials exporter"
}
```

### Workitem example

```json
{
  "activityId": "ID OF THE ACTIVITY",
  "arguments": {
    "inputFile": {
      "url": "URL TO DOWNLOAD THE INPUT FILE (BIM360/ACC)",
      "Headers": {
        "Authorization": "Bearer TOKEN"
      }
    },
    "result": {
      "verb": "post",
      "url": "URL TO UPLOAD THE RESULT",
      "Headers": {
        "Content-Type": "application/json"
      }
    },
    "onProgress": {
      "verb": "post",
      "url": "URL TO RECEIVE PROGRESS INFORMATION",
      "Headers": {
        "Content-Type": "application/json"
      }
    },
    "onComplete": {
      "verb": "post",
      "url": "URL TO RECEIVE STATUS AFTER JOB EXECUTION",
      "Headers": {
        "Content-Type": "application/json"
      }
    }
  }
}
```

## License

This sample is licensed under the terms of the [MIT License](http://opensource.org/licenses/MIT). Please see the [LICENSE](LICENSE) file for full details.

## Written by

João Martins [@JooPaulodeOrne2](http://twitter.com/JooPaulodeOrne2), [Forge Partner Development](http://forge.autodesk.com)
