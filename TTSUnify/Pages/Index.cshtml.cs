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

        [BindProperty]
        public List<UserInputModel> UserInputs { get; set; } = default!;

        public IndexModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
            Debug.WriteLine("IndexModel");
        }


        public void OnGet()
        {
        }


        public async Task<IActionResult> OnPostTransformAsync()
        {

            var audioUrls = new List<string>();
            var audioDirectory = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "audio");
            var timeFolder = DateTime.Now.ToString("MMdd_HH-mm-ss");
            var timeFolderDirectory = Path.Combine(audioDirectory, timeFolder);
            var client = TextToSpeechClient.Create();
            var audioFileIndex = 1;

            // UserInputs가 null이거나 비어있으면 빠져나옴
            if (UserInputs == null || UserInputs.Count == 0 || UserInputs[0].Text == null)
            {
                return Page(); // 페이지를 다시 렌더링하거나 다른 응답을 반환
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

        public IActionResult OnPostCombine([FromBody] List<string> filePaths)
        {

            var audioDirectory = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "audio");
            var audioFiles = Directory.GetFiles(audioDirectory, "*.mp3").OrderBy(file => file).ToList();
            var combinedFilePath = Path.Combine(audioDirectory, "combined.mp3");

            // 타겟 샘플레이트 및 채널 설정
            int targetSampleRate = 24000;
            int targetChannels = 1;
            int silenceDurationInMs = 800; // 0.8초

            // MP3 파일 병합을 위한 출력 스트림 설정
            using (var outputStream = new FileStream(combinedFilePath, FileMode.Create))
            using (var mp3Writer = new LameMP3FileWriter(outputStream, new WaveFormat(targetSampleRate, targetChannels), LAMEPreset.VBR_90))
            {
                foreach (var file in audioFiles)
                {
                    // MP3 파일 복사
                    using (var mp3Reader = new Mp3FileReader(file))
                    {
                        mp3Reader.CopyTo(mp3Writer);
                    }

                    // 무음 삽입
                    InsertSilence(mp3Writer, targetSampleRate, targetChannels, silenceDurationInMs);
                }
            }

            // 최종 MP3 파일의 URL 반환
            return new JsonResult(new { combinedFileUrl = "/audio/combined.mp3" });
        }

        private string AudioCombine(string timeFolderDirectory, string timeFolder)
        {
            // 타겟 샘플레이트 및 채널 설정
            int targetSampleRate = 24000;
            int targetChannels = 1;
            int silenceDurationInMs = 800; // 0.8초
            var combinedFileName = "combined.mp3";

            var audioFiles = Directory.GetFiles(timeFolderDirectory, "*.mp3").OrderBy(file => file).ToList();
            var combinedFilePath = Path.Combine(timeFolderDirectory, combinedFileName);

            // MP3 파일 병합을 위한 출력 스트림 설정
            using (var outputStream = new FileStream(combinedFilePath, FileMode.Create))
            using (var mp3Writer = new LameMP3FileWriter(outputStream, new WaveFormat(targetSampleRate, targetChannels), LAMEPreset.VBR_90))
            {
                foreach (var file in audioFiles)
                {
                    // MP3 파일 복사
                    using (var mp3Reader = new Mp3FileReader(file))
                    {
                        mp3Reader.CopyTo(mp3Writer);
                    }

                    // 무음 삽입
                    InsertSilence(mp3Writer, targetSampleRate, targetChannels, silenceDurationInMs);
                }
            }

            return $"/audio/{timeFolder}/{combinedFileName}";
        }

        private void InsertSilence(LameMP3FileWriter writer, int sampleRate, int channels, int durationInMs)
        {
            int bytesPerSample = 2 * channels; // 16비트 PCM이므로 2바이트
            int silenceSamples = (sampleRate * durationInMs) / 1000;
            byte[] silenceBuffer = new byte[silenceSamples * bytesPerSample];

            writer.Write(silenceBuffer, 0, silenceBuffer.Length);
        }
    }
    
}
