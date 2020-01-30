#region 
/*
This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program.  If not, see <https://www.gnu.org/licenses/>.
*/
#endregion
using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Runtime.InteropServices;
using System.Globalization;
using Microsoft.Win32;

namespace DragonsSpine
{
	/// <summary>
	/// Summary description for ConsoleWriter.
	/// </summary>
	/// 
	
	public enum ConsoleFlashMode
	{
		NoFlashing,
		FlashOnce,
		FlashUntilResponse,
	}

	public class ConsoleWriter : TextWriter
	{
		#region Variables
		TextWriter writer;
		ConsoleColor color;
		ConsoleFlashMode flashing;
		bool beep;
		#endregion

		#region Construction
		public ConsoleWriter(TextWriter writer, ConsoleColor color, ConsoleFlashMode mode, bool beep) 
		{
			this.color = color;
			this.flashing = mode;
			this.writer = writer;
			this.beep = beep;
		}
		#endregion

		#region Properties
		public ConsoleColor Color
		{
			get { return color; }
			set { color = value; }
		}
		
		public ConsoleFlashMode FlashMode
		{
			get { return flashing; }
			set { flashing = value; }
		}

		public bool BeepOnWrite
		{
			get { return beep; }
			set { beep = value; }
		}
		#endregion

		#region Write Routines
 
		public override Encoding Encoding
		{
			get { return writer.Encoding; }
		}

		protected void Flash()
		{
			switch (flashing)
			{
				case ConsoleFlashMode.FlashOnce:
					WinConsole.Flash(true);
					break;
				case ConsoleFlashMode.FlashUntilResponse:
					WinConsole.Flash(false);
					break;
			}

			if (beep)
				WinConsole.Beep();
		}

		public override void Write(char ch)
		{
			ConsoleColor oldColor = WinConsole.Color;
			try
			{
				WinConsole.Color = color;
				writer.Write(ch);
			}
			finally
			{
				WinConsole.Color = oldColor;
			}
			Flash();
		}

		public override void Write(string s)
		{
			ConsoleColor oldColor = WinConsole.Color;
			try
			{
				WinConsole.Color = color;
				Flash();
				writer.Write(s);
			}
			finally
			{
				WinConsole.Color = oldColor;
			}
			Flash();
		}

		public override void Write(char[] data, int start, int count)
		{
			ConsoleColor oldColor = WinConsole.Color;
			try
			{
				WinConsole.Color = color;
				writer.Write(data, start, count);
			}
			finally
			{
				WinConsole.Color = oldColor;
			}
			Flash();
		}
		#endregion
	}

}
