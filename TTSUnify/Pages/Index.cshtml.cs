using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.IO;
using System.Diagnostics;
using Google.Cloud.TextToSpeech.V1;
using NAudio.Wave;
using NAudio.Lame;
using Microsoft.AspNetCore.Mvc.Rendering;
using Google.Api;
using static Google.Rpc.Context.AttributeContext.Types;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using OpenAI.Audio;
using System.ClientModel;
using OpenAI;
using TTSUnify.Core.Enums;
using Microsoft.CognitiveServices.Speech;
using static System.Net.Mime.MediaTypeNames;

public class UserInputModel
{
	public string Text { get; set; } = default!;
	public string Gender { get; set; } = default!;
}

public enum TTSService
{
    Google,
    Azure,
    OpenAI
}

namespace TTSUnify.Pages
{
	public class IndexModel : PageModel
	{
		private readonly ILogger<IndexModel> _logger;
        private readonly IConfiguration _config;

        [BindProperty]
        public TTSService SelectedTtsService { get; set; } = TTSService.OpenAI;
        public SelectList TtsServicesList { get; set; }

        [BindProperty]
        public string SelectedGender { get; set; }
        public string[] Options { get; set; }

        // ���� Ÿ�Կ� ���� �Լ� ������ ���� ��ųʸ� ����
        private readonly Dictionary<TTSService, Func<Task<IActionResult>>> serviceActions;

		[BindProperty]
		public List<UserInputModel> UserInputs { get; set; } = default!;

		public IndexModel(ILogger<IndexModel> logger, IConfiguration config)
		{
            Debug.WriteLine("IndexModel");
			_logger = logger;
            _config = config;

            TtsServicesList = new SelectList(
                Enum.GetValues(typeof(TTSService))
                    .Cast<TTSService>()
                    .Select(e => new SelectListItem
                    {
                        Value = e.ToString(),
                        Text = e.ToString()
                    }),
                "Value",
                "Text",
                SelectedTtsService.ToString());

            // �� ���� Ÿ�԰� �׿� �ش��ϴ� ó�� �Լ� ����
            serviceActions = new Dictionary<TTSService, Func<Task<IActionResult>>>
			{
				{ TTSService.Google, HandleGoogleService },
				{ TTSService.Azure, HandleAzureService },
				{ TTSService.OpenAI, HandleOpenAIService }
			};

		}
        public void OnGet()
        {
            
        }
        public void OnPostSelectGender()
        {
            if (SelectedGender == "male")
            {
                Options = new[] { OPENAIVOICE.m_Alloy.ToString(), OPENAIVOICE.m_Onyx.ToString() };
            }
            else // female
            {
                Options = new[] { OPENAIVOICE.f_Nova.ToString(), OPENAIVOICE.f_Shimmer.ToString() };
            }
        }

        public async Task<IActionResult> OnPostTransformAsync()
        {

            if (serviceActions.ContainsKey(SelectedTtsService))
            {
                // ���ε� �񵿱� �Լ� ȣ�� �� ��� ��ȯ
                return await serviceActions[SelectedTtsService]();
            }
            else
			{
				// �߸��� ���� ���� ó�� (���� ó��)
				return BadRequest("Invalid service selection.");
			}

		}

		private async Task<IActionResult> HandleGoogleService()
		{
			// Google ���� ó�� ����
			var audioUrls = new List<string>();
			var audioDirectory = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "audio");
			var timeFolder = DateTime.Now.ToString("MMdd_HH-mm-ss") + "_google";
            var timeFolderDirectory = Path.Combine(audioDirectory, timeFolder);
			var client = TextToSpeechClient.Create();
			var audioFileIndex = 1;

			// UserInputs�� null�̰ų� ��������� ��������
			if (UserInputs == null || UserInputs.Count == 0 || UserInputs[0].Text == null)
			{
				return Page(); // �������� �ٽ� �������ϰų� �ٸ� ������ ��ȯ
			}

			if (!Directory.Exists(timeFolderDirectory))
			{
				Directory.CreateDirectory(timeFolderDirectory);
			}

			foreach (var inputModel in UserInputs)
			{
				var input = new SynthesisInput
				{
					Text = inputModel.Text
				};

				var voice = new VoiceSelectionParams
				{
					LanguageCode = "ko-KR",
					Name = inputModel.Gender == "Female" ? "ko-KR-Standard-B" : "ko-KR-Standard-C",
					SsmlGender = inputModel.Gender == "Female" ? SsmlVoiceGender.Female : SsmlVoiceGender.Male
				};

				var audioConfig = new AudioConfig
				{
					AudioEncoding = AudioEncoding.Mp3
				};

				var response = await client.SynthesizeSpeechAsync(input, voice, audioConfig);

				var fileName = $"{audioFileIndex++}_{Guid.NewGuid()}.mp3";
				var filePath = Path.Combine(timeFolderDirectory, fileName);
				await System.IO.File.WriteAllBytesAsync(filePath, response.AudioContent.ToByteArray());

				audioUrls.Add($"/audio/{timeFolder}/{fileName}");
			}

			audioUrls.Add(AudioCombine(timeFolderDirectory, timeFolder));

			return new JsonResult(audioUrls);
		}

		private async Task<IActionResult> HandleAzureService()
		{
			// Azure ���� ó�� ����
			Debug.WriteLine("Azure ���� ó�� ����");

            var audioUrls = new List<string>();
            var audioDirectory = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "audio");
            var timeFolder = DateTime.Now.ToString("MMdd_HH-mm-ss") + "_azure";
            var timeFolderDirectory = Path.Combine(audioDirectory, timeFolder);
            var speechConfig = SpeechConfig.FromSubscription(_config["TTS:Azure_speech_key"], "koreacentral");
            var audioFileIndex = 1;

            // UserInputs�� null�̰ų� ��������� ��������
            if (UserInputs == null || UserInputs.Count == 0 || UserInputs[0].Text == null)
            {
                return Page(); // �������� �ٽ� �������ϰų� �ٸ� ������ ��ȯ
            }

            if (!Directory.Exists(timeFolderDirectory))
            {
                Directory.CreateDirectory(timeFolderDirectory);
            }


            foreach (var inputModel in UserInputs)
            {
                var inputText = inputModel.Text;

                speechConfig.SpeechSynthesisVoiceName = inputModel.Gender == "Female" ? "ko-KR-JiMinNeural" : "ko-KR-GookMinNeural";
                speechConfig.SetSpeechSynthesisOutputFormat(SpeechSynthesisOutputFormat.Audio16Khz32KBitRateMonoMp3);

                using var speechSynthesizer = new SpeechSynthesizer(speechConfig,null);

                var speechSynthesisResult = await speechSynthesizer.SpeakTextAsync(inputText);


                var fileName = $"{audioFileIndex++}_{Guid.NewGuid()}.mp3";
                var filePath = Path.Combine(timeFolderDirectory, fileName);
                await System.IO.File.WriteAllBytesAsync(filePath, speechSynthesisResult.AudioData);

                audioUrls.Add($"/audio/{timeFolder}/{fileName}");
            }

            audioUrls.Add(AudioCombine(timeFolderDirectory, timeFolder,16000));

            return new JsonResult(audioUrls);
        }

		private async Task<IActionResult> HandleOpenAIService()
		{
			// OpenAI ���� ó�� ����
			Debug.WriteLine("OpenAI ���� ó�� ����");

            var audioUrls = new List<string>();
            var audioDirectory = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "audio");
            var timeFolder = DateTime.Now.ToString("MMdd_HH-mm-ss") + "_openai";
            var timeFolderDirectory = Path.Combine(audioDirectory, timeFolder);
			OpenAIClient client = new OpenAIClient(new ApiKeyCredential(_config["TTS:OpenAI_demotts_key"]));
            AudioClient ttsClient = client.GetAudioClient("tts-1");
            var audioFileIndex = 1;

            // UserInputs�� null�̰ų� ��������� ��������
            if (UserInputs == null || UserInputs.Count == 0 || UserInputs[0].Text == null)
            {
                return Page(); // �������� �ٽ� �������ϰų� �ٸ� ������ ��ȯ
            }

            if (!Directory.Exists(timeFolderDirectory))
            {
                Directory.CreateDirectory(timeFolderDirectory);
            }

            foreach (var inputModel in UserInputs)
            {
				var inputText = inputModel.Text;

				var voiceType = inputModel.Gender == "Female" ? GeneratedSpeechVoice.Nova: GeneratedSpeechVoice.Alloy;

                BinaryData speech = await ttsClient.GenerateSpeechAsync(inputText, voiceType);

                var fileName = $"{audioFileIndex++}_{Guid.NewGuid()}.mp3";
                var filePath = Path.Combine(timeFolderDirectory, fileName);
                await System.IO.File.WriteAllBytesAsync(filePath, speech.ToArray());

                audioUrls.Add($"/audio/{timeFolder}/{fileName}");
            }

            audioUrls.Add(AudioCombine(timeFolderDirectory, timeFolder));

            return new JsonResult(audioUrls);
        }

		public IActionResult OnPostCombine([FromBody] List<string> filePaths)
		{

			var audioDirectory = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "audio");
			var audioFiles = Directory.GetFiles(audioDirectory, "*.mp3").OrderBy(file => file).ToList();
			var combinedFilePath = Path.Combine(audioDirectory, "combined.mp3");

			// Ÿ�� ���÷���Ʈ �� ä�� ����
			int targetSampleRate = 24000;
			int targetChannels = 1;
			int silenceDurationInMs = 800; // 0.8��

			// MP3 ���� ������ ���� ��� ��Ʈ�� ����
			using (var outputStream = new FileStream(combinedFilePath, FileMode.Create))
			using (var mp3Writer = new LameMP3FileWriter(outputStream, new WaveFormat(targetSampleRate, targetChannels), LAMEPreset.VBR_90))
			{
				foreach (var file in audioFiles)
				{
					// MP3 ���� ����
					using (var mp3Reader = new Mp3FileReader(file))
					{
						mp3Reader.CopyTo(mp3Writer);
					}

					// ���� ����
					InsertSilence(mp3Writer, targetSampleRate, targetChannels, silenceDurationInMs);
				}
			}

			// ���� MP3 ������ URL ��ȯ
			return new JsonResult(new { combinedFileUrl = "/audio/combined.mp3" });
		}

		private string AudioCombine(string timeFolderDirectory, string timeFolder, int samplerate = 24000)
		{
			// Ÿ�� ���÷���Ʈ �� ä�� ����
			int targetSampleRate = samplerate;
			int targetChannels = 1;
			int silenceDurationInMs = 800; // 0.8��
			var combinedFileName = "combined.mp3";

			var audioFiles = Directory.GetFiles(timeFolderDirectory, "*.mp3").OrderBy(file => file).ToList();
			var combinedFilePath = Path.Combine(timeFolderDirectory, combinedFileName);

			// MP3 ���� ������ ���� ��� ��Ʈ�� ����
			using (var outputStream = new FileStream(combinedFilePath, FileMode.Create))
			using (var mp3Writer = new LameMP3FileWriter(outputStream, new WaveFormat(targetSampleRate, targetChannels), LAMEPreset.VBR_90))
			{
				foreach (var file in audioFiles)
				{
					// MP3 ���� ����
					using (var mp3Reader = new Mp3FileReader(file))
					{
						mp3Reader.CopyTo(mp3Writer);
					}

					// ���� ����
					InsertSilence(mp3Writer, targetSampleRate, targetChannels, silenceDurationInMs);
				}
			}

			return $"/audio/{timeFolder}/{combinedFileName}";
		}

		private void InsertSilence(LameMP3FileWriter writer, int sampleRate, int channels, int durationInMs)
		{
			int bytesPerSample = 2 * channels; // 16��Ʈ PCM�̹Ƿ� 2����Ʈ
			int silenceSamples = (sampleRate * durationInMs) / 1000;
			byte[] silenceBuffer = new byte[silenceSamples * bytesPerSample];

			writer.Write(silenceBuffer, 0, silenceBuffer.Length);
		}
	}

}
