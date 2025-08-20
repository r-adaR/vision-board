import numpy as np
import cv2 as cv
import copy
import random
import math
import tensorflow as tf  # TensorFlow Lite uses tf.lite.Interpreter


interpreter = tf.lite.Interpreter(model_path="PieceClassifierHard4.tflite")
interpreter.allocate_tensors()

# Get input/output details
input_details = interpreter.get_input_details()
output_details = interpreter.get_output_details()

# print("Input details:", input_details)
# print("Output details:", output_details)


def classify_piece(input_tile_image, certainty_threshold=0.98, class_dict={0:'', 1:'X', 2:'O', None:'?'}):
    # Convert to float32
    data = np.array(input_tile_image, dtype=np.float32)

    # Resize to 40x40 if not already
    data = cv.resize(data, (40, 40), interpolation=cv.INTER_AREA)

    # Add channel dimension (1, 40, 40)
    if len(data.shape) == 2:
        data = np.expand_dims(data, axis=0)  # channel-first

    # Add batch dimension (1, 1, 40, 40)
    data = np.expand_dims(data, axis=0)

    # Ensure dtype
    data = data.astype(input_details[0]["dtype"])

    # Run inference
    interpreter.set_tensor(input_details[0]['index'], data)
    interpreter.invoke()
    prediction = interpreter.get_tensor(output_details[0]['index'])[0]

    if np.max(prediction) < certainty_threshold:
        return None

    tile_prediction = np.argmax(prediction)
    return class_dict[tile_prediction]



def read_board(input_overhead_image, class_dict={0:'', 1:'X', 2:'O', None:'?'}):
    # Calculate tile size.
    height, width = input_overhead_image.shape
    height, width = (int(height / 5), int(width/5))

    # Split image into tiles and classify each piece.
    final_board = np.zeros((5,5), dtype=str)
    for y in range(5):
        for x in range(5):
            img_section = input_overhead_image[y*height:(y+1)*height, x*width:(x+1)*width]

            predicted_tile = classify_piece(img_section, class_dict=class_dict)

            final_board[y,x] = predicted_tile

    return final_board


def are_boards_equal(board_1, board_2):
    board_1_copy = copy.deepcopy(board_1)
    for i in range(4):
        if np.all(board_1_copy == board_2):
            return True
        board_1_copy = np.rot90(board_1_copy)
    return False
