import socket
import cv2 as cv
import base64
from VisionBoard import vision_board_reader, BoardReadError

# Establish localhost and port.
HOST = "127.0.0.1"
PORT = 8181

with socket.socket(socket.AF_INET, socket.SOCK_STREAM) as s:
    # Bind the socket to the address and port and add a listener to accept connections.
    s.bind((HOST, PORT))
    s.listen(1)
    conn, addr = s.accept()

    # open camera
    camera = cv.VideoCapture(0)
    if not camera.isOpened():
        raise Exception("Could not open camera.")
    
    with conn:
        print('Connected by', addr)
        while True:
            # read frame from camera
            ret, frame = camera.read()

            data = conn.recv(1024)
            if data == b"SCF":
                if not ret:
                    print("Failed to read frame from camera.")
                    break

                # print("Camera frame requested.")
                resized_img = cv.resize(frame, (320, 240))
                img_encode = cv.imencode('.jpg', resized_img)[1].tobytes()
                
                base64_bytes = base64.b64encode(img_encode)
                with open("output.txt", "wb") as f:
                    f.write(base64_bytes)
                conn.sendall(img_encode)

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
                    
                conn.sendall(boardState)
            if data == b"QUIT":
                break



    camera.release()
