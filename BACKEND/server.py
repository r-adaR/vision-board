import socket

HOST_ADDRESS = "127.0.0.1"
PORT = 8181

with socket.socket() as server:
    server.bind((HOST_ADDRESS, PORT))
    server.listen()
    connection, address = server.accept()
    
    with connection:
        print(f"Connected by {address}")
        while True:
            data = connection.recv(1024)
            if not data:
                break
            connection.sendall(data)
