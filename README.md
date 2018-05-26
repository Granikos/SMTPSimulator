# SMTP Simulator

SMTP Simulator is a SMTP service for testing inbound and outbound mailflow.

The SMTP Simulator service helps to test any SMTP based MTA (e.g. Exchange Server) and its related add-ons (e.g. anti-virus, anti-spam, etc.)

This is even more helpful when testing in a closed lab environment without any connection to an external MTA.

The SMTP Simulator is controlled using a web based interface.

![SMTP Simulator Web Interface](https://github.com/Granikos/SMTPSimulator/blob/master/Images/SMTPSimulator-Homepage.png)

## Functionality

The main component of the SMTP Simulator is a Windows service which acts as 

* SMTP Server for sending mail messages, either manually triggered or based on timer settings 
* SMTP Client for accepting SMTP connections from external MTAs 

The overall configuration is performend using a web interface which interacts with the Windows service using a web service interface on TCP port _9001_.

![SMTP Simulator Service Overview](https://github.com/Granikos/SMTPSimulator/blob/master/Images/SMTPSimulator-Overview.png)

## Solution Components

(in alphabetical order)

### Core

* Defines core functions and properties for all other components.

### ImapClient

* IMAP4 client to connect to email servers. 

### Service

* The Windows service for hosting the *SMTP Simulator* service
* The service relies on configurations handled by the *WebClient* component 

### Service.ConfigurationService

* Configuration web service implementation used by WebClient component. 

### Service.Database

* Database interface for storing SMTP service configuration data

### Service.Models

* Object definitions used by the SMTP service

### SmtpClient

* SMTP client component providing send connector functionality to connect to other MTAs

### SmtpServer

* SMTP server component providing receive connector functionality

### Test

* Component providing tests and fakes

### WebClient

* The web interface to manage alls configurations of the SMTPSimulator service

### Setup

* Visual Studio Setup project, requires *Microsoft Studio 2017 Installer Projects* extension
* This component is supposed to be replaced by a proper installer configuration.

## Requirements

The deployment itself is currently a partial manual deployment. I am looking for some to volunteer to have this fully automated.

### Manual installs

* The installer for the Windows service requires the [Microsoft Visual Studio 2017 Installer Projects](https://marketplace.visualstudio.com/items?ItemName=VisualStudioProductTeam.MicrosoftVisualStudio2017InstallerProjects) extension for Visual Studio 2017.
* For getting and bundling the frontend dependencies, [NPM](https://nodejs.org/en/) is required.
  The project also uses [bower](https://bower.io/) and [gulp](http://gulpjs.com/) which are installed via NPM.
  Once you have setup NPM, run `npm install` within the `Granikos.SMTPSimulator.WebClient` folder.
  This installs the necessary tools to build the frontend dependencies.
  Visual Studio should then automatically run the necessary gulp tasks on building the web frontend.
  You can also manually run `gulp` within the `Granikos.SMTPSimulator.WebClient` folder to get the same effect.
  If the `gulp` command fails, make sure that gulp and bower are installed globally.
  You can do this by running `npm install -g gulp bower`.
* The service also requires [SQL Server Compact 4.0](https://www.microsoft.com/en-us/download/details.aspx?id=17876).
* Manual Website Setup, as described in the SMTPSimulator Wiki
* To run the unit tests you will need the *Enterprise Edition* of Visual Studio, because the tests use
  [Microsoft Fakes](https://msdn.microsoft.com/en-us/library/hh549175.aspx).

## Work to do

This project still requires attention and there is more to be done.

You want to participate in this piece of work? Fork the project and let me know.