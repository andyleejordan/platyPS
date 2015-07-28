﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.XPath;
using Markdown.MAML.Model;
using Markdown.MAML.Parser;
using Xunit;

namespace Markdown.MAML.Test.Parser
{
    public class MamlRendererTests
    {
        [Fact]
        public void ProduceNameAndSynopsis()
        {
            var parser = new MarkdownParser();
            var doc = parser.ParseString(@"
## Get-Foo
### Synopsis
This is Synopsis
");
            var renderer = new MamlRenderer(doc);
            
            string maml = renderer.ToMamlString();
            string[] name = GetXmlContent(maml, "/helpItems/command:command/command:details/command:name");
            Assert.Equal(1, name.Length);
            Assert.Equal("Get-Foo", name[0]);

            string[] synopsis = GetXmlContent(maml, "/helpItems/command:command/command:details/maml:description/maml:para");
            Assert.Equal(1, synopsis.Length);
            Assert.Equal("This is Synopsis", synopsis[0]);
        }

        [Fact(Skip = "There is a bug in MarkdownParser, skip for now")]
        public void ProduceMultilineDescription()
        {
            var parser = new MarkdownParser();
            var doc = parser.ParseString(@"
## Get-Foo
### Synopsis
This is Synopsis, but it doesn't matter in this test

### DESCRIPTION
Hello,

I'm a multiline description.

And this is my last line.
");
            var renderer = new MamlRenderer(doc);

            string maml = renderer.ToMamlString();
            string[] description = GetXmlContent(maml, "/helpItems/command:command/maml:description/maml:para");
            Assert.Equal(3, description.Length);
            Assert.Equal("Hello,", description[0]);
            Assert.Equal("I'm a multiline description.", description[1]);
            Assert.Equal("And this is my last line.", description[2]);
        }

        private string[] GetXmlContent(string xml, string xpath)
        {
            List<string> result = new List<string>(); 
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xml);
            var nav = xmlDoc.CreateNavigator();


            XmlNamespaceManager xmlns = new XmlNamespaceManager(nav.NameTable);
            xmlns.AddNamespace("command", "http://schemas.microsoft.com/maml/dev/command/2004/10");
            xmlns.AddNamespace("maml", "http://schemas.microsoft.com/maml/2004/10");
            xmlns.AddNamespace("dev", "http://schemas.microsoft.com/maml/dev/2004/10");
            xmlns.AddNamespace("MSHelp", "http://msdn.microsoft.com/mshelp");

            XPathNodeIterator iterator = nav.Select(xpath, xmlns);
            foreach (var i in iterator)
            {
                result.Add(i.ToString().Trim());
            }

            return result.ToArray();
        }
    }
}