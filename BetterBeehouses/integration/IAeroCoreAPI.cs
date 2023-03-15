using Microsoft.Xna.Framework.Graphics;
using System;

namespace BetterBeehouses.integration
{
	public interface IAeroCoreAPI
	{
		public event Action<SpriteBatch> OnDrawingWorld;
	}
}
