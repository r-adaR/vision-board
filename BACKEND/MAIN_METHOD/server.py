import socket
import cv2 as cv
import base64
from VisionBoard import vision_board_reader

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
            if not ret:
                break

            data = conn.recv(200000)
            if data == b"SCF":
                print("Camera frame requested.")
                #img_encode = cv.imencode('.jpg', frame)[1].tobytes()
                # frame = cv.imread("training_data/coyote.jpg")
                resized_img = cv.resize(frame, (320, 240))
                img_encode = cv.imencode('.jpg', resized_img)[1].tobytes()
                
                base64_bytes = base64.b64encode(img_encode)
                with open("output.txt", "wb") as f:
                    f.write(base64_bytes)
                # print(base64_bytes)
                # Send the Base64-encoded image in chunks
                # chunk_size = 4096  # 4 KB chunks
                # for i in range(0, len(base64_bytes), chunk_size):
                #     conn.sendall(base64_bytes[i:i+chunk_size])
                conn.sendall(img_encode)
            if data == b"RGS":
                boardState = vision_board_reader(frame).tobytes()
                conn.sendall(boardState)
            else:
                print(data)
            if not data:
                break
            # else:
            #     conn.sendall("incorrect protocol?")


    camera.release()
