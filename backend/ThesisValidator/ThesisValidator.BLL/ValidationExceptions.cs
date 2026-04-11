using System.Net;

namespace ThesisValidator.BLL;

public class TemplateNotFoundException(string templateId) : Exception($"Valideerimismalli '{templateId}' ei leitud");

public class UnsupportedFormatException(string extension) : Exception($"Faililaiend '{extension}' ei ole toetatud");
