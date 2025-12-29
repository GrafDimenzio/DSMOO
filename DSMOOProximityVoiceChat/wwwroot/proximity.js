let ws = null;
let localStream = null;
const peers = {};
const audioElements = {};

document.getElementById("joinBtn").addEventListener("click", joinVoiceChat);

window.addEventListener("beforeunload", () => {
    cleanupAll();
    if (ws != null && ws.readyState === WebSocket.OPEN) {
        ws.close(1000, "");
    }
});

async function joinVoiceChat() {
    cleanupAll()
    if(ws) {
        ws.close();
        ws = null;
    }
    try {
        localStream = await navigator.mediaDevices.getUserMedia({
            audio: {
                echoCancellation: true,
                noiseSuppression: true,
                autoGainControl: true,
                channelCount: 1,
            },
            video: false,
        });   
    } catch (error) {
        console.error(error);
        return;
    }

    const url = getWebSocketUrl("/ws/proximity-chat");
    console.log(url);
    ws = new WebSocket(url);

    ws.onmessage = async (e) => {
        const msg = JSON.parse(e.data);
        await handleSignal(msg)
    }

    ws.onopen = () => {
        console.log("WebSocket connected!");
    };

    ws.onclose = () => {
        console.log("WebSocket closed");
        cleanupAll();
    };
}

async function handleSignal(msg) {
    switch (msg.type) {
        case "user-joined":
            await callUser(msg.id);
            break;

        case "offer":
            await handleOffer(msg);
            break;

        case "answer":
            await handleAnswer(msg);
            break;

        case "ice":
            await handleIce(msg);
            break;

        case "volume":
            await handleVolume(msg);
            break;

        case "user-left":
            cleanup(msg.id);
            break;
    }
}

function createPeer(username) {
    const pc = new RTCPeerConnection({
        iceServers: [{urls: "stun:stun.l.google.com:19302"}]
    });

    localStream.getTracks().forEach(track => {
        pc.addTrack(track, localStream);
    });

    pc.onicecandidate = e => {
        if (e.candidate) {
            ws.send(JSON.stringify({
                type: "ice",
                to: username,
                candidate: e.candidate,
            }));
        }
    };

    pc.ontrack = e => {
        const audio = document.createElement("audio");
        audio.autoplay = true;
        audio.srcObject = e.streams[0];
        audio.volume = 1;
        document.body.appendChild(audio);

        audioElements[username] = audio;
    }

    peers[username] = pc;
    return pc;
}

async function callUser(username) {
    const pc = createPeer(username);
    const offer = await pc.createOffer();
    await pc.setLocalDescription(offer);

    ws.send(JSON.stringify({
        type: "offer",
        to: username,
        sdp: offer
    }));
}

async function handleOffer(msg) {
    let pc;
    
    if(peers[msg.from]) {
        pc = peers[msg.from];
    } else {
        pc = createPeer(msg.from);
    }

    const desc = new RTCSessionDescription(msg.sdp);
    await pc.setRemoteDescription(desc);

    if (pc._iceQueue) {
        for (const cand of pc._iceQueue) {
            await pc.addIceCandidate(new RTCIceCandidate(cand));
        }
        delete pc._iceQueue;
    }
    
    const answer = await pc.createAnswer();
    await pc.setLocalDescription(answer);

    ws.send(JSON.stringify({
        type: "answer",
        to: msg.from,
        sdp: pc.localDescription
    }))
}

async function handleAnswer(msg) {
    const pc = peers[msg.from];
    if (!pc) return;
    
    const desc = new RTCSessionDescription(msg.sdp);
    await pc.setRemoteDescription(desc);

    if (pc._iceQueue) {
        for (const cand of pc._iceQueue) {
            await pc.addIceCandidate(new RTCIceCandidate(cand));
        }
        delete pc._iceQueue;
    }
}

async function handleIce(msg) {
    const pc = peers[msg.from];
    if(!pc || !msg.candidate) return;
    
    if(pc.remoteDescription) {
        await pc.addIceCandidate(new RTCIceCandidate (msg.candidate));
    } else {
        pc._iceQueue = pc._iceQueue || [];
        pc._iceQueue.push(msg.candidate);
    }
}

function handleVolume(msg) {
    const audio = audioElements[msg.from];
    if (audio) {
        audio.volume = msg.value;
    }
}

function cleanup(username) {
    if (peers[username]) {
        peers[username].close();
        delete peers[username];
    }
    if (audioElements[username]) {
        audioElements[username].remove();
        delete audioElements[username];
    }
}

function cleanupAll(){
    for (const username in peers) {
        peers[username].close();
        delete peers[username];
    }
    
    for (const username in audioElements) {
        audioElements[username].pause();
        audioElements[username].srcObject = null;
        audioElements[username].remove();
        delete audioElements[username];
    }
    
    if (localStream) {
        localStream.getTracks().forEach(track => track.stop());
        localStream = null;
    }
}