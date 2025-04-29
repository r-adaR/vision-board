import torch
import torch.nn.functional as F
import torchvision.datasets as datasets
import torchvision.transforms as transforms
from torch import optim
from torch import nn
from torch.utils.data import DataLoader, Dataset

import numpy as np
import cv2 as cv

import copy
import random
import math

piece_classifier_model = torch.jit.load('PieceClassifierHard.pt')
piece_classifier_model.eval()
device = "cuda" if torch.cuda.is_available() else "cpu"



def classify_piece(input_tile_image, certainty_threshold=0.98):
    # Push input tile through piece classifier.
    piece_classifier_model.to(device)
    with torch.no_grad():
        data = torch.from_numpy(input_tile_image).unsqueeze(0).unsqueeze(0).to(device)
    
        prediction = piece_classifier_model(data)
        prediction = prediction[0]
    
        prediction = np.array(prediction.detach().cpu().numpy())

    # If too uncertain, return None
    if np.max(prediction) < certainty_threshold:
        return None

    # Return the most confident answer.
    tile_prediction = np.argmax(prediction)
    return tile_prediction



def read_board(input_overhead_image, class_dict={0:'', 1:'X', 2:'O'}):
    # Calculate tile size.
    height, width = input_overhead_image.shape
    height, width = (int(height / 5), int(width/5))

    # Split image into tiles and classify each piece.
    final_board = np.zeros((5,5), dtype=str)
    for y in range(5):
        for x in range(5):
            img_section = input_overhead_image[y*height:(y+1)*height, x*width:(x+1)*width]

            predicted_tile = classify_piece(img_section)

            # Too uncertain, return None
            if predicted_tile is None:
                return None

            # Update read board for this space
            predicted_tile = class_dict[predicted_tile]
            final_board[y,x] = predicted_tile

    return final_board



def are_boards_equal(board_1, board_2):
    board_1_copy = copy.deepcopy(board_1)
    for i in range(4):
        if np.all(board_1_copy == board_2):
            return True
        board_1_copy = np.rot90(board_1_copy)
    return False