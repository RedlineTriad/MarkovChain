using Markov;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MarkovTest
{
	class Program
	{
		const string baseURL = "https://tgstation13.org/parsed-logs/terry/data/logs/";
		const string date = "2019/01/01/";
		const string roundConst = "round-";
		const string fileName = "/game.txt";

		static readonly string[] urls =
		{
			baseURL + date + roundConst + "99555" + fileName,
			baseURL + date + roundConst + "99563" + fileName,
			baseURL + date + roundConst + "99570" + fileName,
			baseURL + date + roundConst + "99577" + fileName,
			baseURL + date + roundConst + "99580" + fileName,
			baseURL + date + roundConst + "99584" + fileName,
			baseURL + date + roundConst + "99587" + fileName,
			baseURL + date + roundConst + "99590" + fileName,
			baseURL + date + roundConst + "99594" + fileName,
			baseURL + date + roundConst + "99595" + fileName,
			baseURL + date + roundConst + "99601" + fileName,
			baseURL + date + roundConst + "99602" + fileName,
			baseURL + date + roundConst + "99603" + fileName,
			baseURL + date + roundConst + "99606" + fileName,
		};

		static void Main(string[] args)
		{
			var chain = new MarkovChain<string>(2);

			PopulateChain(chain);
			Console.WriteLine();
			Generate(chain);
		}

		static void Generate(MarkovChain<string> chain)
		{
			var rand = new Random();
			while (true)
			{
				var sentence = chain.Chain(rand);
				Console.WriteLine(string.Join(" ", sentence));
				Console.ReadLine();
			}
		}

		static void PopulateChain(MarkovChain<string> chain)
		{
			var handler = new HttpClientHandler
			{
				AllowAutoRedirect = false,
				AutomaticDecompression = DecompressionMethods.GZip
			};

			using (var httpClient = new HttpClient(handler))
			{
				var watch = new Stopwatch();
				var downloads = 0;

				urls
				.AsParallel()
				.Select(url => httpClient.GetStreamAsync(url))
				.Select(stream => AliveChat(stream.Result))
				.ForAll(result =>
				{
					Console.Clear();
					Console.WriteLine($"Download Complete: {++downloads}/{urls.Length}"); //Not thread safe, i know
					foreach (var sentence in result)
					{
						watch.Start();
						chain.Add(sentence);
						watch.Stop();
					}
				});
				Console.WriteLine($"Creating the chain took: {watch.ElapsedMilliseconds}ms");
			}
		}

		static IEnumerable<string[]> AliveChat(Stream stream)
		{
			var regex = new Regex(".*SAY: .*\\/(\\([^\\)]*\\) \".*\")");
			using (var sr = new StreamReader(stream))
			{
				string line;
				while ((line = sr.ReadLine()) != null)
				{
					var match = regex.Match(line);
					if (match.Success)
					{
						yield return match.Groups[1].Value.ToLower().Split(' ');
					}
				}
			}
		}
	}
}
