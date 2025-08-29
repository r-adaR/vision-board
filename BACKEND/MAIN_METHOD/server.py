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

    # default loaded camera
    cam_index = 0
    camera = cv.VideoCapture(0)
    if not camera.isOpened():
        print("Could not open camera.")
    
    try:
        print("Server is running. Waiting for instructions...")
        while True:

            # Read frame from camera along with instruction message from the client.
            data, addr = s.recvfrom(1024)
            message = data.decode("utf-8")

            match message:
                # tells client that the server is up and running!
                case "HELLO":
                    s.sendto(b"ACK", addr)


                # If the instruction request is SCF (Send Camera Frame), send the encoded camera frame.
                case "SCF":
                    ret, frame = camera.read()
                    if not ret:
                        print("Failed to read frame from camera.")
                        s.sendto(b"ERR: CAM READ FAILURE", addr)
                        continue

                    resized_img = cv.resize(frame, (320, 240))
                    img_encode = cv.imencode('.jpg', resized_img)[1].tobytes()
                    
                    base64_bytes = base64.b64encode(img_encode)
                    s.sendto(img_encode, addr)


                # If the instruction request is RGS (Read Game State), send the current board state.
                case "RGS":
                    try:
                        ret, frame = camera.read()
                        if not ret:
                            print("Failed to read board from camera")
                            s.sendto(b"ERR: CAM READ FAILURE", addr)
                            continue

                        board = vision_board_reader(frame)

                        for i in range(len(board)):
                            for j in range(len(board[i])):
                                if board[i][j] == '':
                                    board[i][j] = 'E'

                        boardStateString = "".join(board.flatten())
                        boardState = boardStateString.encode("utf-8")
                    except BoardReadError as e:
                        print(e)
                        boardState = b"ERR: BOARD STATE"
                        
                    s.sendto(boardState, addr)


                # If the instruction request is QUIT, break the loop and close the connection.
                case "QUIT":
                    s.sendto(b"GOODBYE!", addr)
                    break


                case _:
                    # If the instruction starts with CAM, switch camera index to the number that follows. Ex: CAM0, CAM1, CAM2
                    if message.startswith("CAM"):
                        try:
                            cam_num: int = int(message[3:])
                            if (cam_num == cam_index):
                                s.sendto(("ERR: "+ message + " ALREADY OPEN").encode("utf-8"), addr)
                                continue;

                            new_camera = cv.VideoCapture(cam_num)
                            if not new_camera.isOpened():
                                print("Failed to open camera "+message[3:])
                                s.sendto(("ERR: "+ message + " CANT OPEN").encode("utf-8"), addr)
                                new_camera.release()
                            else:
                                camera.release()
                                camera = new_camera
                                cam_index = cam_num
                                s.sendto(("CAM" + message[3:] + " OPENED").encode("utf-8"), addr)

                        except ValueError:
                            s.sendto(("ERR: CAM INDEX " + message[3:]).encode("utf-8"), addr)

    finally:
        # Release camera when connection is closed.
        camera.release()
