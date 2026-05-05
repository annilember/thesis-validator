# Student information
* Full name: Anni Lember
* Student code: 233460IADB
* School email: annlem@taltech.ee
* UNI-ID: annlem


# Thesis Validator
### EST:
Prototüüpne veebirakendus akadeemiliste lõputööde struktuuri- ja vormistusnõuete automaatseks valideerimiseks. Realiseeritud TalTech IT-teaduskonna lõputöö vormistusjuhendi põhjal.
### ENG:
Prototypal web application for automated validation of structure and formatting requirements of academic theses. Implemented based on the TalTech IT Faculty thesis formatting guidelines.

# Tech stack

* Frontend: Vue.js
* Backend: ASP.NET Core (C#)
* Parsing: Open XML SDK
* PDF rendering: Syncfusion DocIO
* Language detection: lingua-dotnet
* Containerization: Docker


# Getting started

## Live demo
https://thesis-validator-frontend-production.up.railway.app/

## Local development

### Prerequisites
- .NET 10
- Node.js 20+
- Docker (optional)

### Backend
* `cd backend/ThesisValidator`
* `dotnet run --project ThesisValidator.Api`

### Frontend
* `cd frontend/thesis-validator-frontend`
* `npm install`
* `npm run dev`

### Docker
* `docker-compose up -d`
* `docker-compose down`
