
$(document).ready(function () {
    // Open on load to make testing easier
    document.body.classList.add("show-chatbot");
});

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

const chatbotToggler = document.querySelector(".chatbot-toggler");
const closeBtn = document.querySelector(".close-btn");
const chatbox = document.querySelector(".chatbox");
const chatInput = document.querySelector(".chat-input textarea");
const sendChatBtn = document.querySelector(".chat-input span");

let userMessage = null;
const inputInitHeight = chatInput.scrollHeight;

const createChatLi = (message, className) => {
    const chatLi = document.createElement("li");
    chatLi.classList.add("chat", `${className}`);
    let chatContent = className === "outgoing" ? `<p></p>` : `<span class="material-symbols-outlined">smart_toy</span><p></p>`;
    chatLi.innerHTML = chatContent;
    chatLi.querySelector("p").textContent = message;
    return chatLi;
}

const generateResponse = (message, chatElement) => {
    const messageElement = chatElement.querySelector("p");

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

            //document.getElementById('chat-input').value = '';
            messageElement.textContent = data.response;
            chatbox.scrollTo(0, chatbox.scrollHeight);
        })
        .catch(error => console.error('Error:', error));
}

const handleChat = () => {
    //const message = document.getElementById('chat-input').value;
    //if (message.trim() === '') return;

    userMessage = chatInput.value.trim();
    if (!userMessage) return;

    // Clear the input textarea and set its height to default
    chatInput.value = "";
    chatInput.style.height = `${inputInitHeight}px`;

    // Append the user's message to the chatbox
    chatbox.appendChild(createChatLi(userMessage, "outgoing"));
    chatbox.scrollTo(0, chatbox.scrollHeight);

    setTimeout(() => {
        // Display "Thinking..." message while waiting for the response
        const incomingChatLi = createChatLi("Thinking...", "incoming");
        chatbox.appendChild(incomingChatLi);
        chatbox.scrollTo(0, chatbox.scrollHeight);
        generateResponse(userMessage, incomingChatLi);
    }, 600);
}

chatInput.addEventListener("input", () => {
    // Adjust the height of the input textarea based on its content
    chatInput.style.height = `${inputInitHeight}px`;
    chatInput.style.height = `${chatInput.scrollHeight}px`;
});

chatInput.addEventListener("keydown", (e) => {
    // If Enter key is pressed without Shift key and the window
    // width is greater than 800px, handle the chat
    if (e.key === "Enter" && !e.shiftKey && window.innerWidth > 800) {
        e.preventDefault();
        handleChat();
    }
});

sendChatBtn.addEventListener("click", handleChat);
"show-chatbot"
closeBtn.addEventListener("click", () => document.body.classList.remove("show-chatbot"));
chatbotToggler.addEventListener("click", () => document.body.classList.toggle("show-chatbot"));
