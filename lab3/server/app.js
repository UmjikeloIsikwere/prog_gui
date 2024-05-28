const net = require('net');

const PORT = 12345;
let clients = [];

const server = net.createServer((socket) => {
    socket.on('data', (data) => {
        console.log('Received message:', data.toString());
        let message = data.toString().split("|");
        let messageType = message[0];
        switch (messageType) {
            case "CON":
                let connectedNames = [];
                clients.push({nickname:message[1],socket});
                clients.forEach((client) => {
                    if (client.socket !== socket) {
                        connectedNames.push(client.nickname);
                        client.socket.write(data);
                    }
                });
                if(connectedNames.length > 0){
                    socket.write(`NCA|${connectedNames.join(",")}`);
                }
                break;
            case "SMSG":
                console.log(message);
                clients.forEach((client)=>{
                    if(client.nickname == message[2] || message[2] == "All"){
                        client.socket.write(`RMSG|${message[1]}|${message[3]}`)
                    }
                })
                break;
            default:
                console.log(`Unsupported message: ${message}`);
                break;
        }
    });

    socket.on('end', () => {
        console.log('Client disconnected');
        let userIndex = 0;
        clients.forEach((client, index)=> {
            if(client == socket){
                userIndex = index;
            }
        })
        clients.forEach((client)=> {
            if(client != socket){
                client.socket.write(`DISC|${clients[userIndex].nickname}`)
            }
        })
        if (userIndex !== -1) {
            clients.splice(userIndex, 1);
        }
    });

    socket.on('error', (err) => {
        console.error('Socket error:', err);
    });
});


server.listen(PORT, () => {
    console.log(`Server is listening on port ${PORT}`);
});

server.on('error', (err) => {
    console.error('Server error:', err);
});
