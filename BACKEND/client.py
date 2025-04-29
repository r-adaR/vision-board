import socket

HOST = "127.0.0.1"  # The server's hostname or IP address
PORT = 8181  # The port used by the server

with socket.socket(socket.AF_INET, socket.SOCK_STREAM) as s:
    s.connect((HOST, PORT))
    inp = ""
    while inp != "q":
        inp = input("Enter a message to the server (q to quit): ")
        if inp == "q":
            break
        s.sendall(inp.encode("utf-8"))
        data = s.recv(1024)
        data_decoded = data.decode("utf-8")
        print(f"Received {data_decoded!r}")