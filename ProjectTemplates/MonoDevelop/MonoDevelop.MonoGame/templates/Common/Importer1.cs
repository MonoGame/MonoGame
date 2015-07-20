using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;

using TImport = System.String;

namespace ${Namespace}
{
	[ContentImporter(".txt", DisplayName = "Importer1", DefaultProcessor = "Processor1")]
	public class Importer1 : ContentImporter<TImport>
	{
		public override TImport Import(string filename, ContentImporterContext context)
		{
			return default(TImport);
		}
	}
}

