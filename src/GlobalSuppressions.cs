// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1633:File should have header", Justification = "No header needed its open source!")]
[assembly: SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1200:Using directive should appear within a namespace declaration", Justification = "I like using before namespace")]
[assembly: SuppressMessage("StyleCop.CSharp.SpacingRules", "SA1011:Closing square brackets should be spaced correctly", Justification = "Rule is broken for nullable arrays, see https://github.com/DotNetAnalyzers/StyleCopAnalyzers/issues/3572", Scope = "member", Target = "~M:TerminalGuiDesigner.Operations.DragOperation.#ctor(TerminalGuiDesigner.Design,System.Int32,System.Int32,TerminalGuiDesigner.Design[])")]
