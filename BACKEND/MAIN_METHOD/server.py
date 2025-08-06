import socket
import cv2 as cv
import base64
from VisionBoard import vision_board_reader, BoardReadError

# Establish localhost and port.
HOST = "127.0.0.1"
PORT = 8181

with socket.socket(socket.AF_INET, socket.SOCK_DGRAM) as s:
    # Bind the socket to the address and port and add a listener to accept connections.
    print("Initiating server...")
    s.bind((HOST, PORT))

    # Attempt to open camera
    camera = cv.VideoCapture(0)
    if not camera.isOpened():
        raise Exception("Could not open camera.")
    
    try:
        print("Server is running. Waiting for instructions...")
        while True:
            # Read frame from camera along with instruction message from the client.
            data, addr = s.recvfrom(1024)
            ret, frame = camera.read()

            # If the instruction request is SCF (Send Camera Frame), send the encoded camera frame.
            if data == b"SCF":
                if not ret:
                    print("Failed to read frame from camera.")
                    break

                resized_img = cv.resize(frame, (320, 240))
                img_encode = cv.imencode('.jpg', resized_img)[1].tobytes()
                
                base64_bytes = base64.b64encode(img_encode)
                with open("output.txt", "wb") as f:
                    f.write(base64_bytes)
                s.sendto(img_encode, addr)

            # If the instruction request is RGS (Read Game State), send the current board state.
            if data == b"RGS":
                try:
                    board = vision_board_reader(frame)

                    for i in range(len(board)):
                        for j in range(len(board[i])):
                            if board[i][j] == '':
                                board[i][j] = 'E'

                    boardStateString = "".join(board.flatten())
                    boardState = boardStateString.encode("utf-8")
                except BoardReadError as e:
                    print(e)
                    boardState = b"ERROR"
                    
                s.sendto(boardState, addr)

            # If the instruction request is QUIT, break the loop and close the connection.
            if data == b"QUIT":
                break
    finally:
        # Release camera when connection is closed.
        camera.release()
