///

// 텍스트 영역 크기 자동 조절 스크립트
const textarea = document.getElementById('autoResize');

textarea.addEventListener('input', function () {
    // 텍스트를 입력할 때마다 높이를 0으로 설정하여 높이를 다시 계산함
    this.style.height = 'auto';
    // 스크롤 높이에 맞춰서 높이를 설정
    this.style.height = (this.scrollHeight) + 'px';
});

// 페이지가 로드될 때 초기 크기 조정
window.addEventListener('load', function () {
    textarea.style.height = (textarea.scrollHeight) + 'px';
});


document.addEventListener('DOMContentLoaded', function () {
    let inputCount = 1;

    document.getElementById('addButton').addEventListener('click', function () {
        const inputContainer = document.getElementById('inputContainer');
        const newInputGroup = document.createElement('div');
        newInputGroup.className = 'input-group';

        const textarea = document.createElement('textarea');
        textarea.id = 'autoResize';
        textarea.name = `UserInputs[${inputCount}].Text`;
        textarea.className = 'text-input';
        textarea.placeholder = 'Enter text here';

        const genderInputs = document.createElement('div');
        genderInputs.className = 'gender-inputs';

        const maleLabel = document.createElement('label');
        const maleRadio = document.createElement('input');
        maleRadio.type = 'radio';
        maleRadio.name = `UserInputs[${inputCount}].Gender`;
        maleRadio.value = 'Male';
        maleRadio.checked = true;
        maleLabel.appendChild(maleRadio);
        maleLabel.appendChild(document.createTextNode(' 남성'));

        const femaleLabel = document.createElement('label');
        const femaleRadio = document.createElement('input');
        femaleRadio.type = 'radio';
        femaleRadio.name = `UserInputs[${inputCount}].Gender`;
        femaleRadio.value = 'Female';
        femaleLabel.appendChild(femaleRadio);
        femaleLabel.appendChild(document.createTextNode(' 여성'));

        genderInputs.appendChild(maleLabel);
        genderInputs.appendChild(femaleLabel);

        newInputGroup.appendChild(textarea);
        newInputGroup.appendChild(genderInputs);
        inputContainer.appendChild(newInputGroup);

        inputCount++;
    });

    document.getElementById('ttsForm').addEventListener('submit', function (event) {
        event.preventDefault();

        const formData = new FormData(this);

        fetch('@Url.Page("Index", "Transform")', {
            method: 'POST',
            body: formData
        })
            .then(response => response.json())
            .then(audioUrls => {
                const audioContainer = document.getElementById('audioContainer');
                audioContainer.innerHTML = '';
                audioUrls.forEach(function (audioUrl) {
                    const audioElement = document.createElement('div');
                    audioElement.innerHTML = `<audio controls src="${audioUrl}"></audio>`;
                    audioContainer.appendChild(audioElement);
                });
            })
            .catch(() => {
                alert("An error occurred while processing your request.");
            });
    });

    document.getElementById('combineButton').addEventListener('click', function () {
        const csrfToken = document.querySelector('input[name="__RequestVerificationToken"]').value;

        fetch('@Url.Page("Index", "Combine")', {
            method: 'POST',
            headers: {
                'RequestVerificationToken': csrfToken,
                'Content-Type': 'application/json; charset=utf-8'
            }
        })
            .then(response => response.json())
            .then(result => {
                if (result.combinedFileUrl) {
                    const combinedAudioElement = document.createElement('div');
                    combinedAudioElement.innerHTML = `<audio controls src="${result.combinedFileUrl}"></audio>`;
                    document.getElementById('audioContainer').appendChild(combinedAudioElement);
                }
            })
            .catch(() => {
                alert("An error occurred while processing your request.");
            });
    });
});
