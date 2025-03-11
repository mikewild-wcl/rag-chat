// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

document.onload = function () {
    console.log('loaded');
};

document.getElementById('chat-input').addEventListener("keypress", function (event) {
    if (event.key === "Enter") {
        event.preventDefault();
        document.getElementById("send-button").click();
    }
});

document.getElementById('send-button').addEventListener('click', function () {
    const message = document.getElementById('chat-input').value;
    if (message.trim() === '') return;

    fetch('/api/chat', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify({ message: message })
    })
        .then(response => response.json())
        .then(data => {
            const chatMessages = document.getElementById('chat-messages');
            const userMessage = document.createElement('div');
            userMessage.textContent = `You: ${message}`;
            chatMessages.appendChild(userMessage);

            const botMessage = document.createElement('div');
            botMessage.textContent = `Bot: ${data.response}`;
            chatMessages.appendChild(botMessage);

            document.getElementById('chat-input').value = '';
        })
        .catch(error => console.error('Error:', error));
});
