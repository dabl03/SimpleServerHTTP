using System;
using System.Net;
using System.Text.Json;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;
using System.IO;

namespace SimpleServerHTTP;
/// <sumary>
/// Informacion para editar HTML por medio de funciones, y poder editar las plantillas.
/// @todo: Crear una comunicacion cliente/servidor.
/// </summay>
[AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
public class DomInfo: Attribute {
	public DomInfo(){

	}
	
}