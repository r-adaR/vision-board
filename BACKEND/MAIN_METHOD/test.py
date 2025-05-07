# from VisionBoard import vision_board_reader
import cv2 as cv
import base64


if __name__ == '__main__':
    # for i in range(10, 19):
    #     print(f"easy_{i}.png")
    #     test_image = cv.imread(f"training_data/Easy/easy_{i}.png")
    #     board = vision_board_reader(test_image)
    #     for i in range(len(board)):
    #         for j in range(len(board[i])):
    #             if board[i][j] == '':
    #                 board[i][j] = ' '
    #     print(board)

    frame = None
    camera = cv.VideoCapture(1)
    if not camera.isOpened():
        raise Exception("Could not open camera.")
    while True:
        ret, frame = camera.read()
        if not ret:
            break
        print(frame)
        cv.imshow("Camera", frame)
        key = cv.waitKey(1)
        if key == ord('q'):
            break
    camera.release()


    img_encode = cv.imencode('.jpg', frame)[1].tobytes()
    base64_bytes = base64.b64encode(img_encode)
    print(base64_bytes)
