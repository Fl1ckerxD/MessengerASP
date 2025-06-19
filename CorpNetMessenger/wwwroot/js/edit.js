document.getElementById('AvatarFile').addEventListener('change', (e) => {
    const file = e.target.files[0];
    if (file) {
        const reader = new FileReader();
        reader.onload = function (event) {
            document.querySelector('.profile-avatar').src = event.target.result;
        }
        reader.readAsDataURL(file);
    }
});

document.addEventListener("DOMContentLoaded", () => {
    const imgButton = document.querySelector('.profile-avatar');
    const avatarFile = document.getElementById('AvatarFile');

    imgButton.addEventListener('click', () => {
        avatarFile.click();
    });
});