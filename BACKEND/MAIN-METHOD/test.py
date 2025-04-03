from VisionBoard import vision_board_reader
import cv2 as cv

if __name__ == '__main__':
    test_image = cv.imread("training_data/Hard/hard_385.png")
    board = vision_board_reader(test_image)
    for i in range(len(board)):
        for j in range(len(board[i])):
            if board[i][j] == '':
                board[i][j] = ' '
    print(board)