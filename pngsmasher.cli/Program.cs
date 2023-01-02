using APNGLib;
using BigGustave;
using ImageMagick;
using pngsmasher.Core;
using System;
using System.Diagnostics;
using System.Reflection;
using static pngsmasher.Core.Types;
using static pngsmasher.Core.Utils;
using static pngsmasher.Core.Corruption;
using ColorType = ImageMagick.ColorType;
using Pixel = BigGustave.Pixel;

namespace pngsmasher.CLI
{
	static class Program
	{
		static CLIOptions options = new CLIOptions();
		private static Type _stringType = typeof(string);
		private static Type _intType = typeof(int);
		private static Type _floatType = typeof(float);
		private static Type _boolType = typeof(bool);
		private static Logging logging;

		static void VerbWrite(object str)
		{
			Debug.WriteLine(str);

			if (!options.silent && options.verbose)
			{
				Console.WriteLine(str);
			}
		}

		static void Write(object str, bool dontWriteLine = false)
		{
			if (!options.silent)
				if (dontWriteLine)
					Console.Write(str);
				else
					Console.WriteLine(str);
		}

		static void WError(object str)
		{
			if (!options.silent)
			{
				ConsoleColor old = Console.ForegroundColor;
				Console.ForegroundColor = ConsoleColor.Red;
				Console.Error.WriteLine(str);
				Console.ForegroundColor = old;
			}
		}

		static void WWarn(object str)
		{
			if (!options.silent)
			{
				ConsoleColor old = Console.ForegroundColor;
				Console.ForegroundColor = ConsoleColor.Yellow;
				Console.WriteLine(str);
				Console.ForegroundColor = old;
			}
		}

		static void Main(string[] args)
		{
			logging = new Logging(VerbWrite, Write, WWarn, WError);

			// parse cli args
			PropertyInfo[] @params = options.GetType().GetProperties();
			Dictionary<string, PropertyInfo> vlist = new();
			Dictionary<string,bool> vlistAssigned = new();

			foreach (PropertyInfo fi in @params)
			{
				var attr = fi.GetCustomAttribute<CLIValueAttribute>();
				var key = attr?.ParameterName ?? fi.Name;

				vlist.Add(key, fi);
				vlistAssigned.Add(key, false);
			}

			string paramName = "";

			for (int i = 0; i < args.Length; i++)
			{
				string arg = args[i];
				if (paramName == "")
				{
					if (arg[0] == '-')
					{
						paramName = arg[1..];

						if (!vlist.ContainsKey(paramName))
						{
							WError("Unknown parameter " + arg + "!");
							paramName = "";
						}
						else
						{
							if (vlist[paramName].PropertyType == _boolType)
							{
								if (vlistAssigned[paramName])
								{
									WWarn("Parameter encountered twice in arguments: -" + paramName);
								}

								vlistAssigned[paramName] = true;
								vlist[paramName].SetValue(options, true);
								paramName = "";
							}
						}
						continue;
					}
				}
				else
				{
					PropertyInfo field = vlist[paramName];
					Type type = field.PropertyType;
					object? data = null;

					if (vlistAssigned[paramName])
					{
						WWarn("Parameter encountered twice in arguments: -" + paramName);
					}

					vlistAssigned[paramName] = true;

					// switch case doesn't like the types not being constant ssssooooooooo
					if (type == _stringType)
						data = Convert.ToString(arg);
					else if (type == _intType)
					{
						int data_sub;
						if (!int.TryParse(arg, out data_sub))
						{
							WError("Failed to parse int argument " + arg + " for parameter -" + paramName);
							continue;
						}
						else
						{
							data = data_sub;
						}
					}
					else if (type == _floatType)
					{
						float data_sub;
						if (!float.TryParse(arg, out data_sub))
						{
							WError("Failed to parse float argument " + arg + " for parameter -" + paramName);
							continue;
						}
						else
						{
							data = data_sub;
						}
					}

					paramName = "";
					field.SetValue(options, data);
				}
			}

			Console.ForegroundColor = ConsoleColor.DarkMagenta; Write(@"   ___    _  __  _____   ____   __  ___   ___    ____   __ __   ____   ___ ");
			Console.ForegroundColor = ConsoleColor.Magenta;		Write(@"  / _ \  / |/ / / ___/  / __/  /  |/  /  / _ |  / __/  / // /  / __/  / _ \");
			Console.ForegroundColor = ConsoleColor.Red;			Write(@" / ___/ /    / / (_ /  _\ \   / /|_/ /  / __ | _\ \   / _  /  / _/   / , _/");
			Console.ForegroundColor = ConsoleColor.DarkRed;		Write(@"/_/    /_/|_/  \___/  /___/  /_/  /_/  /_/ |_|/___/  /_//_/  /___/  /_/|_| ");
			Console.ForegroundColor = ConsoleColor.Gray;		Write("--- by andreweathan --- (and a little help from Marioalexsan)\n");

			if (options.seed <= 0)
			{
				int old = options.seed;
				options.seed = options.seed == 0 ? 1 : Math.Abs(options.seed);
				WError("Due to how pngsmasher's seeded RNG works, you can't use values less than or equal to 0.");
				WError("The seed has been changed from " + old + " to " + options.seed);
			}

			if (options.showHelp)
			{
				Write("Welcome to PNGSMASHER!");
				Write("PNGSMASHER arguments:");

				foreach (var param in vlist)
				{
					var attr = param.Value.GetCustomAttribute<CLIValueAttribute>();

					Console.ForegroundColor = ConsoleColor.Red;
					Write("\t| -" + param.Key);
					Console.ForegroundColor = ConsoleColor.Yellow;
					Write("\t|  Usage: " + attr?.Example ?? "No example provided for this :(");
					Console.ForegroundColor = ConsoleColor.Gray;
					Write("\t|  "
						+ (attr?.HelpString ?? "No help string provided for this :(")
						+ Environment.NewLine);
				}

				Write("");

				Console.ForegroundColor = ConsoleColor.Yellow;
				Write("--- Examples of argument usage ---");


				Console.ForegroundColor = ConsoleColor.Green; 
					Write("\tSimple glitching on a single image:");
				Console.ForegroundColor = ConsoleColor.Yellow; 
					Write("\tpngfucker.cli.exe -input input.png -output output.png -splits 2 -regions 2 -rmin -5 -rmax -3 -crunch 25 -blackbg -seed 1\n");

				Console.ForegroundColor = ConsoleColor.Green; 
					Write("\tSimple glitching on a folder of images:");
				Console.ForegroundColor = ConsoleColor.Yellow; 
					Write("\tpngfucker.cli.exe -input mycoolsharks -output mycoolersharks -splits 1 -regions 1 -rmin -4 -rmax -3 -crunch 35 -blackbg -seed 20\n");

				Console.ForegroundColor = ConsoleColor.Green;
					Write("\tSplit-glitching only the lower half of the image");
				Console.ForegroundColor = ConsoleColor.Yellow;
					Write("\tpngfucker.cli.exe -input someball.png -splits 4 -splitmin 50 -crunch 60 -seed 25\n");

				Console.ForegroundColor = ConsoleColor.Gray;
				return;
			}

			List<string> files = new List<string>();
			if (File.Exists(options.input))
			{
				files.Add(options.input);
				Write(options.input + " is a file!");
			}
			else if (Directory.Exists(options.input))
			{
				var subfiles = Directory.EnumerateFiles(options.input);
				Write(options.input + " is a directory, added " + subfiles.Count() + " files inside!");
				files = subfiles.ToList();
			}
			else
			{
				WError("Couldn't find file " + options.input + "!");
				return;
			}

			var timeTotal = new Stopwatch();
			var settings = new MagickReadSettings();
			settings.ColorType = ColorType.TrueColorAlpha;

			if (options.seed == -2147483647)
				options.seed = new Random().Next(-2147483646, 2147483647);

			SeedRand srand = new SeedRand(options.seed);

			Write("Using seed " + options.seed);

			int filesdone = 0;

			timeTotal.Start();
			foreach (string file in files)
			{
				string output = options.output;

				if (files.Count > 1)
				{
					string noext = Path.GetFileNameWithoutExtension(output);
					string ext = Path.GetExtension(output);

					if (ext == "")
					{
						output = Path.Combine(noext, Path.GetFileName(file));
					}
					else
					{
						output = noext + "_" + filesdone + ext;
					}
				}

				if (!options.overwrite)
				{
					string original = output;
					bool ow_warned = false;
					int ow_warns = 0;
					while (File.Exists(output))
					{
						ow_warns++;

						string? path = Path.GetDirectoryName(original);
						string noext = Path.GetFileNameWithoutExtension(original);
						string ext = Path.GetExtension(original);
						output = Path.Combine(path ?? "", noext + "_" + filesdone + "_" + ow_warns + ext);

						ow_warned = true;
					}

					if (ow_warned)
					{
						WWarn("WARN: Output file already exists [" + original + "], writing to [" + output + "] instead!");
						WWarn("WARN: To ignore this warning and overwrite the existing file, pass the argument -o");
					}
				}

				string? dirname = Path.GetDirectoryName(output);
				if (dirname != null && dirname != "")
				{
					try
					{
						Directory.CreateDirectory(dirname);
					}
					catch (Exception e)
					{
						WError("Tried to create invalid directory for output (" + dirname + "), this failure could cause a fatal error");
						WError("[" + e.Message + " - dir: " + dirname + "]");
					}
				}

				bool useMagick = false;
				APNG png = new APNG();
				try
				{
					using (Stream s = File.OpenRead(options.input))
						png.Load(s);
				}
				catch (Exception e)
				{
					useMagick = true;
					VerbWrite("Couldn't open " + options.input + " as APNG, will try with Magick - " + e.Message);
				}

				if (!useMagick)
				{
					if (png.FrameCount < 1)
					{
						VerbWrite(options.input + " is not an APNG, just a PNG.");

						if (options.frames > 1)
						{
							useMagick = false;
							VerbWrite("Corrupting as an APNG.");

							List<byte[]> data = new List<byte[]>();
							byte[] fdata = File.ReadAllBytes(options.input);

							for (int i = 0; i < options.frames; i++)
							{
								data.Add(fdata);
							}

							png = APNGAssembler.AssembleAPNG(data, 1, (ushort)options.fps, true);
						}
						else
						{
							useMagick = true;
							VerbWrite("Using Magick.");
						}
					}
				}

				filesdone++;
				Write(filesdone + ": " + file + " => " + output);

				var timeAll = new Stopwatch();

				timeAll.Start();
					long result = useMagick ? MagickCorrupt(file, output, settings, srand) : APNGCorrupt(png, output, srand);
				timeAll.Stop();

				if (result == -1) continue;

				Write("Finished " + file + " => " + output + " (" + result + " ms corruption, " + (timeAll.ElapsedMilliseconds - result) + " ms i/o)");
			}
		timeTotal.Stop();

			Write("Done! (" + timeTotal.ElapsedMilliseconds + " ms total)");
		}

		// returns time taken to corrupt (i/o time is ignored)
		public static long MagickCorrupt(string file, string output, MagickReadSettings settings, SeedRand srand)
		{
			var timeThis = new Stopwatch();
			using var img = new MagickImage(file, settings);
			img.ColorType = ColorType.TrueColorAlpha;

			var pixels = img.GetPixels();
			var bytes = pixels.ToByteArray(0, 0, img.Width, img.Height, PixelMapping.RGBA);
			if (bytes == null)
			{
				WError("Failed to corrupt image " + file + ": Image byte data was null!");
				return -1;
			}

			timeThis.Start();
			var (rgba, imgwidth, imgheight, _) = OldStyleCorruptImage(bytes, options, srand, img.Width, img.Height, true, logging);
			bytes = rgba;
			timeThis.Stop();

			img.Crop(imgwidth, imgheight);
			pixels = img.GetPixels();
			pixels.SetPixels(bytes);

			img.Write(output);

			return timeThis.ElapsedMilliseconds;
		}

		// returns time taken to corrupt (i/o time is ignored)
		public static long APNGCorrupt(APNG file, string output, SeedRand srand)
		{
			List<byte[]> frames = new();
			List<(ushort, ushort)> delays = new();
			long time = 0;

			for (int i = 0; i < file.FrameCount; i++)
			{
				Frame aframe = file.GetFrame(i);

				var stream = file.ToStream(i);
				stream.Seek(0, SeekOrigin.Begin);

				// kinda ugly
				Png frame = Png.Open(stream);
				byte[] bytes = new byte[file.Width * file.Height * 4];

				int xoff = (int)aframe.XOffset;
				int yoff = (int)aframe.YOffset;

				for (int y = 0; y < frame.Height; y++)
				{
					for (int x = 0; x < frame.Width; x++)
					{
						Pixel pix = frame.GetPixel(x, y);

						int idx = ((x + xoff) + (y + yoff) * (int)file.Width) * 4;
						bytes[idx++] = pix.R;
						bytes[idx++] = pix.G;
						bytes[idx++] = pix.B;
						bytes[idx++] = pix.A;
					}
				}

				var timeThis = new Stopwatch();
				timeThis.Start();
					(byte[] rgba, int imgwidth, int imgheight, bool tookABreak) = OldStyleCorruptImage(bytes, options, srand, (int)file.Width, (int)file.Height, options.verbose, logging);
					bytes = rgba;
				timeThis.Stop();
				time += timeThis.ElapsedMilliseconds;

				// set apng frame delays
				int fps = (int)(options.defaultFPSOnBreak ? (tookABreak ? 1 / (aframe.DelayNumerator / (float)aframe.DelayDenominator) : options.fps) : options.fps);
				ushort num = (ushort)(fps > -1 ? 1 : aframe.DelayNumerator);
				ushort denom = (ushort)(fps > -1 ? fps : aframe.DelayDenominator);
				delays.Add((num, denom));

				var builder = PngBuilder.Create(imgwidth, imgheight, true);
				for (int j = 0; j < bytes.Length; j += 4)
				{
					int x = (j / 4) % imgwidth;
					int y = (j / 4) / imgwidth;

					builder.SetPixel(
						new Pixel(
							bytes[j],
							bytes[j + 1],
							bytes[j + 2],
							bytes[j + 3],
							false
						), x, y
					);
				}

				builder.SetPixel((byte)srand.Generate(0, 255), (byte)srand.Generate(0, 255), (byte)srand.Generate(0, 255), 2, 2);

				var data = builder.Save(new PngBuilder.SaveOptions() { AttemptCompression = true });
				frames.Add(data);
			}

			APNG result = APNGAssembler.AssembleAPNG(frames, delays, true);

			using (var filestream = File.Create(output))
			{
				var res = result.ToStream();
				res.Seek(0, SeekOrigin.Begin);
				res.CopyTo(filestream);
			}

			return time;
		}
	}
}