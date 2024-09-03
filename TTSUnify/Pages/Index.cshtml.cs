using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.IO;
using System.Diagnostics;
using Google.Cloud.TextToSpeech.V1;
using NAudio.Wave;
using NAudio.Lame;

public class UserInputModel
{
    public string Text { get; set; } = default!;
    public string Gender { get; set; } = default!;
}

namespace TTSUnify.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly TextToSpeechClient _textToSpeechClient;

        [BindProperty]
        public List<UserInputModel> UserInputs { get; set; } = default!;

        public IndexModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
            Debug.WriteLine("IndexModel");
        }

        private void DeleteAllAudioFiles()
        {
            var _audioDirectory = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "audio");

            if (Directory.Exists(_audioDirectory))
            {
                var audioFiles = Directory.GetFiles(_audioDirectory, "*.mp3");

                foreach (var file in audioFiles)
                {
                    try
                    {
                        System.IO.File.Delete(file);
                    }
                    catch (IOException ex)
                    {
                        // ���� ���� �� ���ܰ� �߻��ϸ� ó�� (��: ������ ���� �ִ� ���)
                        // �α׸� ����ų�, ����ڿ��� �˸� �� �ֽ��ϴ�.
                        // ���ܸ� �����ϰ� ��� ������ �õ��� ���� �ֽ��ϴ�.
                        Console.WriteLine($"Error deleting file {file}: {ex.Message}");
                    }
                }
            }
        }

        public void OnGet()
        {
        }


        public async Task<IActionResult> OnPostAsync()
        {


            // ����� ���� ����
            DeleteAllAudioFiles();

            var audioUrls = new List<string>();
            var audioDirectory = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "audio");
            var client = TextToSpeechClient.Create();

            if (!Directory.Exists(audioDirectory))
            {
                Directory.CreateDirectory(audioDirectory);
            }

            foreach (var inputModel in UserInputs)
            {
                var input = new SynthesisInput
                {
                    Text = inputModel.Text
                };

                // ������ ���� ���� ����
                var voice = new VoiceSelectionParams
                {
                    LanguageCode = "ko-KR",  // �ѱ��� ����
                    Name = inputModel.Gender == "Female" ? "ko-KR-Standard-B" : "ko-KR-Standard-C",
                    SsmlGender = inputModel.Gender == "Female" ? SsmlVoiceGender.Female : SsmlVoiceGender.Male
                };

                var audioConfig = new AudioConfig
                {
                    AudioEncoding = AudioEncoding.Mp3
                };

                var response = await client.SynthesizeSpeechAsync(input, voice, audioConfig);

                var fileName = $"{Guid.NewGuid()}.mp3";
                var filePath = Path.Combine(audioDirectory, fileName);
                await System.IO.File.WriteAllBytesAsync(filePath, response.AudioContent.ToByteArray());

                audioUrls.Add($"/audio/{fileName}");
            }

            return new JsonResult(audioUrls);
        }

        public void OnGetFoo()
        {
            Debug.WriteLine("get foo");
        }

        public IActionResult OnPostCombine()
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

        private void InsertSilence(LameMP3FileWriter writer, int sampleRate, int channels, int durationInMs)
        {
            int bytesPerSample = 2 * channels; // 16��Ʈ PCM�̹Ƿ� 2����Ʈ
            int silenceSamples = (sampleRate * durationInMs) / 1000;
            byte[] silenceBuffer = new byte[silenceSamples * bytesPerSample];

            writer.Write(silenceBuffer, 0, silenceBuffer.Length);
        }
    }
    
}
