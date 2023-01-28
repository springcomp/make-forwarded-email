using Spectre.Console;
using Spectre.Console.Cli;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace make_fwd
{
	internal sealed class MakeForwardedCommand : Command<MakeForwardedCommand.Settings>
	{
		public sealed class Settings : CommandSettings
		{
			[CommandOption("-s|--sender")]
			[Description("The sender (reply-to) email address. Defaults to no-reply@masked-emails.me.")]
			public string? From { get; set; } = "no-reply@masked-emails.me";

			[CommandArgument(0, "<recipient>")]
			[Description("The recipient email address to which the message is forwarded.")]
			public string? To { get; set; }

			[CommandArgument(1, "<messagePath>")]
			[Description("Path to the mime message to forward.")]
			public string? Message { get; set; }

			[CommandOption("-o|--output")]
			[Description("Path to a file where the forwarded mime message will be written to. Defaults to STDOUT.")]
			public string? OutputPath { get; set; } = "<STDOUT>";

			public override ValidationResult Validate()
			{
				if (Message == null)
					return ValidationResult.Error("Missing required `messagePath` parameter. Please, specify the valid path to a mime message file.");

				if (!File.Exists(Message))
					return ValidationResult.Error("Path to `messagePath` file not found or invalid. Please, specify the valid path to a mime message file.");

				if (!String.IsNullOrEmpty(OutputPath) && OutputPath != "<STDOUT>")
				{
					var folder = Path.GetDirectoryName(OutputPath);
					if (!Directory.Exists(folder))
						return ValidationResult.Error("Value for `--output` path not found or invalid. Please, specify the valid path to an output file in a valid folder.");
				}

				return ValidationResult.Success();
			}
		}

		public override int Execute([NotNull] CommandContext context, [NotNull] Settings settings)
		{
			var from = settings.From;
			var to = settings.To;
			var message = settings.Message;

			var output = settings.OutputPath;

			if (output != "<STDOUT>")
			{
				Console.WriteLine(from);
				Console.WriteLine(to);
				Console.WriteLine(message);
				Console.WriteLine(output);
			}

			System.Diagnostics.Debug.Assert(from != null);
			System.Diagnostics.Debug.Assert(to != null);
			System.Diagnostics.Debug.Assert(message != null);
			System.Diagnostics.Debug.Assert(output != null);

			var helper = new MakeForwardedMessageHelper(from, to);
			var forwarded = helper.ForwardTo(message);

			if (output == "<STDOUT>")
			{
				// redirect console output

				using (var memoryStream = new MemoryStream())
				{
					forwarded.WriteTo(memoryStream);
					Console.Out.Write(Encoding.ASCII.GetString(memoryStream.ToArray()));
				}
			}

			else
			{
				var file = Path.GetFileName(message);
				var path = Path.Join(output, file);

				using (var fileStream = File.Open(path, FileMode.Create, FileAccess.Write, FileShare.Read))
					forwarded.WriteTo(fileStream);
			}

			return 0;
		}
	}
}
