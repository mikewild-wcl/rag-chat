
$(function () {
    // Open on load to make testing easier
    console.log('document ready...');
    document.body.classList.add("show-chatbot");
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
        .then(response => {
            if (response.status === 429 || response.status === 503) {
                console.log('responded');
                console.log('too many requests!');
                console.log(`server responded with status ${response.status}`);

                messageElement.textContent = 'The server is busy. Please try again later.';
                chatbox.scrollTo(0, chatbox.scrollHeight);
                return null;
            //    return new Promise(function (resolve, reject) {
            //        resolve('The server is busy. Please try again later.');
            //    })
            }

            if (!response.ok) {
                throw new Error('Network response was not ok');
            }

            return response.json();
        })
        .then(data => {
            console.log('in the data handler...' + data);                       
            if (!data) return;

            messageElement.textContent = data.response;
            chatbox.scrollTo(0, chatbox.scrollHeight);
        })
        .catch(error => {
            console.log('in the error handler...')
            console.error('Error:', error)
        });
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
        //generateResponse(userMessage, incomingChatLi);
        sendToServer(userMessage, incomingChatLi);
    }, 600);
}

/* Code for streaming response */
// Send prompt to server and stream the response back as tokens are received
async function sendToServer(message, chatElement) {
    //var payload = {
    //    prompt: message
    //}
    var payload = {
        message: message
    }

    console.log(`sending message ${message}`);

    const response = await fetch('/api/chat-stream', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
        },
        body: JSON.stringify(payload),
    });

    var responseText = '';
    const messageElement = chatElement.querySelector("p");
    messageElement.textContent = '';

    for await (const chunk of streamAsyncIterator(response.body)) {
        if (!chunk) continue;

        console.log(`streamAsyncIterator item: ${chunk}`);
        var parsedChunk = JSON.parse(chunk);
        messageElement.textContent += parsedChunk.content;
        chatbox.scrollTo(0, chatbox.scrollHeight);
    }
}

// this might not be needed
function appendMessage(message) {
    var chatContainer = document.getElementById('chat-container');
    var messageElement = document.createElement('div');
    messageElement.textContent = message;
    chatContainer.appendChild(messageElement);
    chatContainer.scrollTop = chatContainer.scrollHeight;
    return messageElement;
}

// Streams - https://web.dev/articles/streams
// https://okankaradag.com/en/net-6-0/streaming-json-response-with-iasyncenumerable-in-net-6-0-and-example-fetch-in-javascript
//https://stackoverflow.com/questions/78900183/how-to-split-chunks-into-individual-json-objects
// Example using IAsyncEnumerable
// Simple polyfill since StreamResponse still can't be used as iterator by most browsers
async function* streamAsyncIterator(stream) {
    const reader = stream.getReader();
    try {
        const decoder = new TextDecoder();
        const debugTarget = document.getElementById('chat-debug');

        while (true) {
            const { done, value } = await reader.read();
            console.log(`streamAsyncIterator done: ${done} - value: ${value}`);
            if (done) return;

            var item = decoder.decode(value, { stream: true }).replace(/\[|]/g, '').replace(/^,/, '');

            console.log(`streamAsyncIterator item: ${item}`);

            //for (const v of item.split(/(?<=\})\s*(?=\{)/)) {
            //    console.log("have a split by whitespace");
            //    debugTarget.innerHTML += `<p>split by whitespace: ${v}</p>`;
            //}

            for (const contentChunk of item.split(/(?<=\}),(?=\{)/)) {
                //console.log(JSON.parse(v).id);
                if (debugTarget) {
                    debugTarget.innerHTML += `<p>split by comma: ${contentChunk}</p>`;
                    debugTarget.scrollTo(0, debugTarget.scrollHeight);
                }

                yield contentChunk;
            }
        }
    }
    finally {
        reader.releaseLock();
    }
}
/* --END Code for streaming response*/  

chatInput.addEventListener("input", () => {
    // Adjust the height of the input textarea based on its content
    chatInput.style.height = `${inputInitHeight}px`;
    chatInput.style.height = `${chatInput.scrollHeight}px`;
});

chatInput.addEventListener("keydown", (e) => {
    console.log('key pressed');
    if (e.key === "Enter" && !e.shiftKey) {
        e.preventDefault();
        handleChat();
    }
});

sendChatBtn.addEventListener("click", handleChat);
closeBtn.addEventListener("click", () => document.body.classList.remove("show-chatbot"));
chatbotToggler.addEventListener("click", () => document.body.classList.toggle("show-chatbot"));
