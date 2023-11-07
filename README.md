# Bold Reporting Services

This Bold Reporting Services repository contains the [ASP.NET MVC Services](https://documentation.boldreports.com/javascript/report-viewer/report-service/create-aspnet-web-api-service/) for the `Report Viewer` and `Report Designer` controls. We have built Bold Reports demos and documentation samples using this service application.

This section guides you to use the Syncfusion Reporting Services in your applications.

* [Requirements to run the service](#requirements-to-run-the-service)
* [Using the Reporting Services](#using-the-reporting-services)
* [Testing the Reporting Services](#testing-the-reporting-services)
* [Online Demos](#online-demos)
* [Documentation](#documentation)
* [License](#license)
* [License key](#license-key)
* [Support and Feedback](#support-and-feedback)

## Requirements to run the service

The samples requires the below requirements to run.

* [Visual Studio 2019](https://visualstudio.microsoft.com/downloads/)
* .Net Framework 4.5 and above

## Using the Reporting Services

* Open the solution file `ReportsSamplesService.sln` in Visual Studio.

* The following Reports dependencies will be installed automatically at compile time.

Package | Purpose
--- | ---
`BoldReports.Web` | Builds the server-side implementations.
`BoldReports.JavaScript` | contains reporting components scripts and style sheets.
`BoldReports.Extensions.BarcodeCRI` | Used for Rendering Barcode.

* Build and host your application in IIS.

* After hosting in IIS, use the below services for running Reports samples.

Control | Service URL
--- | ---
`Report Viewer` | `http://localhost/{{IIS virtual path Name}}/api/ReportViewerWebApi`
`Report Designer` | `http://localhost/{{IIS virtual path Name}}/api/ReportDesignerWebApi`

## Testing the Reporting Services

* Open the solution file `ReportsSamplesService.sln` in Visual Studio.

* Press `F5` or click the `Run` button in Visual Studio to launch the application.

* Navigate to `http://localhost:{{Port No}}/Samples/ReportViewer.html` to test the Report Viewer service.

* Navigate to `http://localhost:{{Port No}}/Samples/ReportDesigner.html` to test the Report Designer service.

## Online Demos

We have showcased our Reports Services in the following Bold Reports online demos.

* [Bold Report JavaScript Reports demos](https://demos.boldreports.com/home/index.html)
* [Bold Reports Angular Reports demos](https://demos.boldreports.com/home/angular.html)
* [Bold Reports ASP.NET MVC Reports demos](https://demos.boldreports.com/home/aspnet-mvc.html)
* [Bold Reports ASP.NET WebForms Reports demos](https://demos.boldreports.com/home/aspnet-web-forms.html)

## Documentation

A complete Syncfusion Reports documentation can be found on [Bold Reports Help](https://documentation.boldreports.com/).

## License

Refer the [LICENSE](/LICENSE) file.

## License key

You have to update the `BoldLicense.txt` with your Bold Reports license key to seamlessly run this application without any license validation errors. To know more about the Bold Reports licensing, refer the link [here](https://documentation.boldreports.com/licensing/).

## Support and Feedback

* For any other queries, reach our [Bold Reports support team](mailto:support@boldreports.com) or [Feedback portal](https://www.boldreports.com/feedback/).

* To renew the subscription, click [here](https://www.boldreports.com/pricing/on-premise) or contact our sales team at <https://www.boldreports.com/contact>.
