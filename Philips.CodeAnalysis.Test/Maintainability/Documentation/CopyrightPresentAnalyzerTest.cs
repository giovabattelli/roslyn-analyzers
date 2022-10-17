﻿using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Philips.CodeAnalysis.Common;
using Philips.CodeAnalysis.MaintainabilityAnalyzers.Documentation;

namespace Philips.CodeAnalysis.Test.Maintainability.Documentation
{
	[TestClass]
	public class CopyrightPresentAnalyzerTest : DiagnosticVerifier
	{
		#region Non-Public Data Members

		private const string ExpectedFixedHeader = @"#region Header
// ©
#endregion
";

		#endregion

		#region Non-Public Properties/Methods

		private const string configuredCompanyName = @"Koninklijke Philips N.V.";

		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
		{
			return new CopyrightPresentAnalyzer();
		}

		protected override Dictionary<string, string> GetAdditionalAnalyzerConfigOptions()
		{
			Dictionary<string, string> options = new Dictionary<string, string>
			{
				{ $@"dotnet_code_quality.{ Helper.ToDiagnosticId(DiagnosticIds.CopyrightPresent) }.company_name", configuredCompanyName  }
			};
			return options;
		}

		#endregion

		#region Public Interface

		[DataRow(@"#region H
			#endregion", false, 2)]
		[DataRow(@"#region Header
#endregion", false, 2)]
		[DataRow(@"#region Header
// ©
#endregion", false, 2)]
		[DataRow(@"#region Header
// © Koninklijke Philips N.V.
#endregion", false, 2)]
		[DataRow(@"#region Header
// © 2021
#endregion", false, 2)]
		[DataRow(@"#region Header
// © Koninklijke Philips N.V. 2021
#endregion", true, 2)]
		[DataRow(@"#region © Koninklijke Philips N.V. 2021
//
// All rights are reserved. Reproduction or transmission in whole or in part,
// in any form or by any means, electronic, mechanical or otherwise, is
// prohibited without the prior written consent of the copyright owner.
//
// Filename: Dummy.cs
//
#endregion", true, 1)]
		[DataRow(@"#region © Koninklijke Philips N.V. 2021
#endregion", true, 1)]
		[DataRow(@"#region Copyright Koninklijke Philips N.V. 2021
#endregion", true, 1)]
		[DataRow(@"#region Koninklijke Philips N.V. 2021
#endregion", false, 2)]
		[DataRow(@"#region Copyright 2021
#endregion", false, 2)]
		[DataRow(@"#region Copyright Koninklijke Philips N.V.
#endregion", false, 2)]
		[DataRow(@"// ©", false, -1)]
		[DataRow(@"// © Koninklijke Philips N.V.", false, -1)]
		[DataRow(@"// © 2021", false, -1)]
		[DataRow(@"// Copyright 2021", false, -1)]
		[DataRow(@"// Copyright Koninklijke Philips N.V. 2021", true, -1)]
		[DataRow(@"/* Copyright Koninklijke Philips N.V. 2021", true, -1)]
		[DataRow(@"// © Koninklijke Philips N.V. 2021", true, -1)]
		[DataRow(@"/* © Koninklijke Philips N.V. 2021", true, -1)]
		[DataRow(@"", false, 2)]
		[DataTestMethod]
		public void HeaderIsDetected(string content, bool isGood, int errorLine)
		{
			string baseline = @"{0}
using Microsoft.VisualStudio.TestTools.UnitTesting;
class Foo 
{{
  public void Foo()
  {{
  }}
}}
";
			string givenText = string.Format(baseline, content);

			DiagnosticResult[] expected;

			if (isGood)
			{
				expected = Array.Empty<DiagnosticResult>();
			}
			else
			{
				expected = new[] { new DiagnosticResult
				{
					Id = Helper.ToDiagnosticId(DiagnosticIds.CopyrightPresent),
					Message = new Regex(".+"),
					Severity = DiagnosticSeverity.Error,
					Locations = new[]
					{
					new DiagnosticResultLocation("Test0.cs", errorLine, 1)
				} }
				};
			}

			VerifyCSharpDiagnostic(givenText, expected);
		}

		[TestMethod]
		public void HeaderIsDetected2()
		{
			string baseline = @"using Microsoft.VisualStudio.TestTools.UnitTesting;
class Foo 
{{
  public void Foo()
  {{
  }}
}}
";
			string givenText = baseline;

			DiagnosticResult[] expected = new[] { DiagnosticResultHelper.Create(DiagnosticIds.CopyrightPresent) };

			VerifyCSharpDiagnostic(givenText, expected);
		}

		[DataRow(@"")]
		[DataRow(@"
")]
		[TestMethod]
		public void EmptyUnitIsIgnored(string text)
		{
			DiagnosticResult[] expected = Array.Empty<DiagnosticResult>();

			VerifyCSharpDiagnostic(text, expected);
		}


		[DataRow(@"// ------
// <auto-generated>
// content
// </auto-generated>
using System;
using System.Reflection;
[assembly: global::System.Runtime.Versioning.TargetFrameworkAttribute()]
", "blah")]
		[DataRow(@"// <auto-generated>
// content
// </auto-generated>
using System;
using System.Reflection;
[assembly: global::System.Runtime.Versioning.TargetFrameworkAttribute()]
", "blah")]
		[DataRow(@"// <auto-generated />
using System;
using System.Reflection;
[assembly: global::System.Runtime.Versioning.TargetFrameworkAttribute()]
", "blah")]
		[DataRow(@"// <autogenerated />
using System;
using System.Reflection;
[assembly: global::System.Runtime.Versioning.TargetFrameworkAttribute()]
", "blah")]
		[DataRow(@"using System;
using System.Reflection;
[assembly: global::System.Runtime.Versioning.TargetFrameworkAttribute()]
", "AssemblyInfo")]
		[DataRow(@"using System;
using System.Reflection;
[assembly: global::System.Runtime.Versioning.TargetFrameworkAttribute()]
", "Foo.AssemblyInfo")]
		[DataRow(@"using System;
using System.Reflection;
[assembly: global::System.Runtime.Versioning.TargetFrameworkAttribute()]
", "Blah.Designer")]
		[DataTestMethod]
		public void AutogeneratedIsIgnored(string text, string filenamePrefix)
		{
			DiagnosticResult[] expected = Array.Empty<DiagnosticResult>();

			VerifyCSharpDiagnostic(text, filenamePrefix, expected);
		}


		[DataRow(@"Foo.Designer.cs")]
		[DataRow(@"Foo.designer.cs")]
		[DataRow(@"Foo.g.cs")]
		[DataTestMethod]
		public void IsGeneratedCaseAgnostic(string text)
		{
			Assert.IsTrue(Helper.IsGeneratedCode(text));
		}

		#endregion
	}
}
