using System;
using System.Collections.Generic;
using System.Linq;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;

namespace PythonLab
{
    public class ReadOnlySectionProvider : IReadOnlySectionProvider
    {
        private readonly TextDocument textDocument;

        public ReadOnlySectionProvider(TextDocument textDocument)
        {
            this.textDocument = textDocument;
        }

        public bool CanInsert(int offset)
        {
            var lastLine = this.textDocument.Lines.Last();

            return offset >= lastLine.Offset + 4;
        }

        public IEnumerable<ISegment> GetDeletableSegments(ISegment segment)
        {
            if (segment == null)
            {
                throw new ArgumentNullException("segment");
            }

            if (CanInsert(segment.Offset))
            {
                yield return segment;
            }
        }
    }
}