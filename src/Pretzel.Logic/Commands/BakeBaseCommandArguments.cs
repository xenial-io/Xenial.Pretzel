using System;
using System.Linq;
using System.Collections.Generic;
using System.CommandLine;
using System.IO.Abstractions;

namespace Pretzel.Logic.Commands
{
    public abstract class BakeBaseCommandArguments : PretzelBaseCommandArguments
    {
        protected BakeBaseCommandArguments(IFileSystem fileSystem) : base(fileSystem) { }

        protected override IEnumerable<Option> CreateOptions() => base.CreateOptions().Concat(new[]
        {
            new Option<bool>("-c", ["--cleantarget"])
            {
                Description = "Delete the target directory (_site by default)"
            },
        });

        public bool CleanTarget { get; set; }
    }
}
