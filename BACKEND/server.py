import socket
import cv2 as cv
import base64
from MAIN_METHOD.VisionBoard import vision_board_reader

# Establish localhost and port.
HOST = "127.0.0.1"
PORT = 8181

with socket.socket(socket.AF_INET, socket.SOCK_STREAM) as s:
    # Bind the socket to the address and port and add a listener to accept connections.
    s.bind((HOST, PORT))
    s.listen(1)
    conn, addr = s.accept()

    # open camera
    camera = cv.VideoCapture(1)
    if not camera.isOpened():
        raise Exception("Could not open camera.")
    


    


    with conn:
        print('Connected by', addr)
        while True:
            # Echo data received back to the client (TODO: Change to send board data).

            # read frame from camera
            ret, frame = camera.read()
            if not ret:
                break


            data = conn.recv(1024)
            if data == b"SCF":
                img_encode = cv.imencode('.jpg', frame)[1].tobytes()
                base64_bytes = base64.b64encode(img_encode)
                # print(base64_bytes)

                break
            if data == b"RGS":
                boardState = vision_board_reader(frame).tobytes()
                conn.sendall(boardState)

                break
            if not data:
                break
            # else:
            #     conn.sendall("incorrect protocol?")


    camera.release()
