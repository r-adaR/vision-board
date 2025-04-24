import socket

# Establish localhost and port.
HOST = "127.0.0.1"
PORT = 8181

with socket.socket(socket.AF_INET, socket.SOCK_STREAM) as s:
    # Bind the socket to the address and port and add a listener to accept connections.
    s.bind((HOST, PORT))
    s.listen(1)
    conn, addr = s.accept()

    with conn:
        print('Connected by', addr)
        while True:
            # Echo data received back to the client (TODO: Change to send board data).
            data = conn.recv(1024)
            if not data:
                break
            conn.sendall(data)
