
using System;

using System.Collections.Generic;

using System.IO;
using System.IO.Abstractions.TestingHelpers;

using Fluid;

using Pretzel.Logic.Templating.Context;

using Xunit;

namespace Pretzel.Tests.Templating.Context
{
    public class DataTests
    {
        private readonly Data data;
        private readonly MockFileSystem fileSystem;
        private readonly string dataDirectory = @"C:\_data";

        public DataTests()
        {
            fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>());
        }

        [Fact]
        public void renders_empty_string_if_data_directory_does_not_exist()
        {
            var template = FluidTemplate.Parse(@"{{ data.people }}");

            var templateContext = CreateTemplateContext();

            var result = template.Render(templateContext);

            Assert.Equal("", result.Trim());
        }

        private TemplateContext CreateTemplateContext()
        {
            var model = new DataContext { data = new Data(fileSystem, dataDirectory) };
            var templateContext = new TemplateContext();
            templateContext.MemberAccessStrategy.Register(model.GetType());
            templateContext.MemberAccessStrategy.Register(typeof(Data));
            templateContext.MemberAccessStrategy.Register(typeof(IDictionary<string, object>));
            templateContext.MemberAccessStrategy.Register(typeof(Dictionary<string, object>));

            templateContext.Model = model;
            return templateContext;
        }

        public class DataContext
        {
            public Data data { get; set; }
        }

        [Theory]
        [InlineData("yml", @"name: Eric Mill")]
        [InlineData("json", @"{ name: 'Eric Mill' }")]
        [InlineData("csv", @"name
""Eric Mill""
")]
        [InlineData("tsv", @"name
""Eric Mill""
")]
        public void renders_nested_object(string ext, string fileContent)
        {
            fileSystem.AddFile(Path.Combine(dataDirectory, $"person.{ext}"), new MockFileData(fileContent));

            var template = FluidTemplate.Parse(@"{{ data.person.name }}");

            var templateContext = CreateTemplateContext();

            var result = template.Render(templateContext);

            Assert.Equal("Eric Mill", result.Trim());
        }

        [Theory]
        [InlineData("yml", @"name: Eric Mill
address:
  street: Some Street
  postalcode: 1234")]
        [InlineData("json", @"{
  name: 'Eric Mill',
  address: {
  street: 'Some Street',
  postalcode: 1234
  }
}")]
        [InlineData("csv", @"name,address.street,address.postalcode
""Eric Mill"",""Some Street"",1234")]
        [InlineData("tsv", @"name	address.street	address.postalcode
""Eric Mill""	""Some Street""	1234")]
        public void renders_deep_nested_object(string ext, string fileContent)
        {
            fileSystem.AddFile(Path.Combine(dataDirectory, $"person.{ext}"), new MockFileData(fileContent));

            var template = FluidTemplate.Parse(@"{{ data.person.address.postalcode }}");

            var templateContext = CreateTemplateContext();

            var result = template.Render(templateContext);

            Assert.Equal("1234", result.Trim());
        }

        [Theory]
        [InlineData("yml", @"- name: Eric Mill
  github: konklone

- name: Parker Moore
  github: parkr

- name: Liu Fengyun
  github: liufengyun")]
        [InlineData("json", @"[{
    name: 'Eric Mill',
    github: 'konklone',
},{
    name: 'Parker Moore',
    github: 'parkr'
},{
    name: 'Liu Fengyun',
    github: 'liufengyun'
}]")]
        [InlineData("csv", @"name,github
""Eric Mill"",""konklone""
""Parker Moore"",""parkr""
""Liu Fengyun"",""liufengyun""")]
        [InlineData("tsv", @"name	github
""Eric Mill""	""konklone""
""Parker Moore""	""parkr""
""Liu Fengyun""	""liufengyun""")]
        public void renders_nested_lists(string ext, string fileContent)
        {
            fileSystem.AddFile(Path.Combine(dataDirectory, $"members.{ext}"), new MockFileData(fileContent));

            var template = FluidTemplate.Parse(@"{{ data.members | size }}");

            var templateContext = CreateTemplateContext();

            var result = template.Render(templateContext);

            Assert.Equal("3", result.Trim());
        }

        [Theory]
        [InlineData("yml", @"dave:
    name: David Smith
    twitter: DavidSilvaSmith")]
        [InlineData("json", @"{
    dave: {
        name: 'David Smith',
        twitter: 'DavidSilvaSmith'
    }
}")]
        //TODO: This is currently not supported. See https://jekyllrb.com/docs/datafiles/#example-accessing-a-specific-author
        //            [InlineData("csv", @"dave.name,dave.twitter
        //""David Smith"",""DavidSilvaSmith""")]
        //            [InlineData("tsv", @"dave.name	dave.twitter
        //""David Smith""	""DavidSilvaSmith""")]
        public void renders_dictionary_accessors(string ext, string fileContent)
        {
            fileSystem.AddFile(Path.Combine(dataDirectory, $"people.{ext}"), new MockFileData(fileContent));

            var template = FluidTemplate.Parse(@"{{ data.people['dave'].name }}");

            var templateContext = CreateTemplateContext();

            var result = template.Render(templateContext);

            Assert.Equal("David Smith", result.Trim());
        }

        [Theory]
        [InlineData("yml", @"name: Eric Mill")]
        [InlineData("json", @"{
    name: 'Eric Mill'
}")]
        [InlineData("csv", @"name
""Eric Mill""")]
        [InlineData("tsv", @"name
""Eric Mill""")]
        public void renders_nested_folder_object(string ext, string fileContent)
        {
            fileSystem.AddFile(Path.Combine(dataDirectory, $@"users\person.{ext}"), new MockFileData(fileContent));

            var template = FluidTemplate.Parse(@"{{ data.users.person.name }}");

            var templateContext = CreateTemplateContext();

            var result = template.Render(templateContext);

            Assert.Equal("Eric Mill", result.Trim());
        }

        [Theory]
        [InlineData("yml", @"name: Eric Mill
email: eric@example.com")]
        [InlineData("json", @"{
    name: 'Eric Mill',
    email: 'eric@example.com'
}")]
        [InlineData("csv", @"name,email
""Eric Mill"",""eric@example.com""")]
        [InlineData("tsv", @"name	email
""Eric Mill""	""eric@example.com""")]
        public void caches_result(string ext, string fileContent)
        {
            fileSystem.Directory.CreateDirectory(dataDirectory);

            var fileName = Path.Combine(dataDirectory, $"person.{ext}");
            fileSystem.AddFile(fileName, new MockFileData(fileContent));

            var template = FluidTemplate.Parse(@"{{ data.person.name }} {{ data.person.email }}");

            var templateContext = CreateTemplateContext();

            var result = template.Render(templateContext);

            Assert.Equal("Eric Mill eric@example.com", result.Trim());
        }

        [Fact]
        public void renders_multiple_files_with_nested_objects()
        {
            fileSystem.AddFile(Path.Combine(dataDirectory, $"eric.yml"), new MockFileData(@"name: Eric Mill"));
            fileSystem.AddFile(Path.Combine(dataDirectory, $"manuel.yml"), new MockFileData(@"name: Manuel Grundner"));

            var templateContext = CreateTemplateContext();

            var templateEric = FluidTemplate.Parse(@"{{ data.eric.name }}");
            var resultEric = templateEric.Render(templateContext);
            Assert.Equal("Eric Mill", resultEric.Trim());

            var templateManuel = FluidTemplate.Parse(@"{{ data.manuel.name }}");
            var resultManuel = templateManuel.Render(templateContext);
            Assert.Equal("Manuel Grundner", resultManuel.Trim());
        }
    }
}
