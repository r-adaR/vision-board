import torch
import torch.nn.functional as F
import torchvision.datasets as datasets
import torchvision.transforms as transforms
from torch import optim
from torch import nn
from torch.utils.data import DataLoader, Dataset

import numpy as np
import cv2 as cv

from scipy.spatial import ConvexHull

from sklearn.cluster import AgglomerativeClustering

import copy
import random
import math

board_locator_model = torch.jit.load('BoardLocatorHard.pt')
board_locator_model.eval()
INPUT_RESOLUTION = (106, 80)
device = "cuda" if torch.cuda.is_available() else "cpu"

def locate_board(input_image, threshold=0.4, blur=0.0):
    is_color = input_image.shape[-1] == 3
    
    # Greyscale image
    image_grey = input_image
    if is_color:
        target_axis = len(input_image.shape) - 1
        image_grey = np.mean(input_image, axis=target_axis)
        
    # Lower Resolution
    image_grey_batch = image_grey
    if len(image_grey.shape) == 2:
        # Make a single image a batch collection of 1.
        image_grey_batch = image_grey_batch[np.newaxis, :, :]

    width = image_grey_batch[0].shape[1]
    height = image_grey_batch[0].shape[0]
    scale_factor = height / INPUT_RESOLUTION[1]
    new_size = ((int)(math.floor(width / scale_factor)), INPUT_RESOLUTION[1])
    image_small = np.zeros((image_grey_batch.shape[0], new_size[1], new_size[0]))
    for i in range(image_grey_batch.shape[0]):
        # Downscale all images.
        image_small[i] = cv.resize(image_grey_batch[i], new_size, interpolation=cv.INTER_AREA)
        image_small[i] = cv.normalize(image_small[i], None, alpha=0, beta=255, norm_type=cv.NORM_MINMAX)

    # Convert to tensor and upload to model.
    input_tensor = torch.from_numpy(image_small.astype(np.float32)).unsqueeze(1).to(device)
    board_locator_model.to(device)
    output_tensor = None
    with torch.no_grad():
        output_tensor = board_locator_model(input_tensor)

    # Convert output tensor to final output.
    output_images = np.matrix(output_tensor.detach().cpu().numpy())
    match len(output_images.shape):
        case 2: # Single image output.
            output_images = output_images[np.newaxis,:,:]
        case 4: # 4D tensor channel output.
            output_images = output_image[:,0,:,:]

    # Upscale output to original image size and threshold.
    final_output = np.zeros((image_grey_batch.shape[0], image_grey.shape[-2], image_grey.shape[-1]))
    for i in range(final_output.shape[0]):
        # Upscale all images.
        final_output[i] = cv.resize(output_images[i], (image_grey.shape[-1], image_grey.shape[-2]), interpolation=cv.INTER_CUBIC) 
        if blur > 0.0:
            blur_size = (int)(min(image_grey.shape[-2], image_grey.shape[-1]) * blur)
            if blur_size % 2 == 0:
                blur_size = blur_size + 1
            if blur_size < 5:
                continue
            final_output[i] = cv.GaussianBlur(final_output[i],(blur_size,blur_size),0)

    final_output = np.where(final_output > np.max(final_output) * threshold, 1, 0)
    if final_output.shape[0] == 1:
        final_output = final_output[0,:,:]

    return final_output

# Calculate corners.
def calculate_intersection(polar_line_1, polar_line_2):
    rho_1, theta_1 = polar_line_1
    rho_2, theta_2 = polar_line_2

    cotan_1 = math.cos(theta_1) / math.sin(theta_1) if not math.sin(theta_1) == 0.0 else 999999999
    cotan_2 = math.cos(theta_2) / math.sin(theta_2) if not math.sin(theta_2) == 0.0 else 999999999
    sin_1 = math.sin(theta_1) if not math.sin(theta_1) == 0.0 else 0.000000001
    sin_2 = math.sin(theta_2) if not math.sin(theta_2) == 0.0 else 0.000000001

    x = ((rho_1 / sin_1) - (rho_2 / sin_2)) / (cotan_1 - cotan_2)
    y = -1 * cotan_1 * x + (rho_1 / sin_1)

    return (x, y)

# Sort corners in known order for the homography.
def convex_hull_sort(points):
    convex_hull_points = ConvexHull(points)
    points_sorted = points[convex_hull_points.vertices]
    start_index = 0
    for i in range(len(points_sorted)):
        if points_sorted[i][1] <= points_sorted[start_index][1]:
            start_index = i
    
    center = np.mean(points_sorted, axis=0)
    if center[0] < points_sorted[start_index][0]:
        start_index = start_index - 1 % len(points_sorted)
    
    points_sorted = np.roll(points_sorted, -1 * start_index, axis=0)
    return points_sorted

# Returns an image if successful, otherwise returns None
def board_to_overhead(original_image):
    # Scale down input image.
    width = original_image.shape[1]
    height = original_image.shape[0]
    scale_factor = height / 480
    new_size = ((int)(math.floor(width / scale_factor)), 480)
    original_image = cv.resize(original_image, new_size, interpolation=cv.INTER_AREA)
    
    # Find edges of board.
    located_board = locate_board(original_image, 0.4, 0.3)
    edge_image = cv.Canny(np.uint8(located_board) * 255, 100, 200)

    # Find lines outlining board.
    lines_ungrouped = cv.HoughLines(edge_image, 1, np.pi / 180, 45, None, 0, 0)

    if lines_ungrouped is None:
        return None




    
    #
    #     Calculate 4 bounding lines.
    #

    
    # Begin grouping lines into 4 lines.    
    distance_threshold = min(original_image.shape[0], original_image.shape[1]) * 0.4
    clustering_model = AgglomerativeClustering(n_clusters=None, distance_threshold=distance_threshold)
    
    # Scale theta value to make distance_threshold more reliable.
    modified_lines_ungrouped = copy.copy(lines_ungrouped)
    if modified_lines_ungrouped is None:
        return None

    theta_scaling_factor = max(original_image.shape[0], original_image.shape[1])
    modified_lines_ungrouped[:,0,1] = theta_scaling_factor * modified_lines_ungrouped[:,0,1] / math.pi

    # Error if we found too few lines to even group into 4 groups.
    if len(modified_lines_ungrouped) < 4:
        return None

    # Group lines
    clustering_model.fit(modified_lines_ungrouped[:,0,:])
    
    # Attempt fitting multiple times in case of near vertical lines around 0 and pi
    attempts = 0
    while not clustering_model.n_clusters_ == 4 and attempts < 2:
        modified_lines_ungrouped[:,0,1] = (modified_lines_ungrouped[:,0,1] + theta_scaling_factor * 0.15)
        for i in range(len(modified_lines_ungrouped)):
            if modified_lines_ungrouped[i,0,1] > theta_scaling_factor:
                modified_lines_ungrouped[i,0,0] = -1 * modified_lines_ungrouped[i,0,0]
                modified_lines_ungrouped[i,0,1] = modified_lines_ungrouped[i,0,1] % theta_scaling_factor
        clustering_model.fit(modified_lines_ungrouped[:,0,:])
        attempts = attempts + 1
    
    # Must have found 4 grouped lines.
    if not clustering_model.n_clusters_ == 4:
        return None

    
    #
    #     Average lines and group by axis along board.
    #

    
    # Group lines.
    lines_grouped = np.zeros((4, lines_ungrouped.shape[-1]))
    label_num = np.zeros((4,))
    line_type = np.zeros((4,), dtype=int)
    for i in range(0, len(lines_ungrouped)):
        lines_grouped[clustering_model.labels_[i]] = lines_grouped[clustering_model.labels_[i]] + modified_lines_ungrouped[i][0]
        label_num[clustering_model.labels_[i]] = label_num[clustering_model.labels_[i]] + 1
        
    slopes = np.zeros((4,2))
    for i in range(4):
        lines_grouped[i] = lines_grouped[i] / label_num[i]
        slopes[i,0] = math.pi * lines_grouped[i,1] / theta_scaling_factor
    
    slope_clustering_model = AgglomerativeClustering(n_clusters=2, linkage='ward')
    
    # Attempt multiple times in case of near vertical lines around 0 and pi
    attempts_slope_grouping = 0
    while(not len(line_type[line_type == 0]) == 2 and not len(line_type[line_type == 1]) == 2 and attempts_slope_grouping < 10):
        slope_clustering_model.fit(slopes)
        for i in range(4):
            line_type[i] = slope_clustering_model.labels_[i]
    
        # Slightly offset angles to hopefully overflow and match previously distant slopes.
        slopes[:,0] = (slopes[:,0] + (math.pi / 8)) % math.pi
        attempts_slope_grouping = attempts_slope_grouping + 1
    
    # Must have 2 vertical and 2 horizontal lines.
    if not len(line_type[line_type == 0]) == 2 or not len(line_type[line_type == 1]) == 2:
        return None

    # Convert scaled lines into original.
    while attempts > 0:
        lines_grouped[:,1] = (lines_grouped[:,1] - theta_scaling_factor * 0.15)
        for i in range(len(lines_grouped)):
            if lines_grouped[i,1] < 0:
                lines_grouped[i,0] = -1 * lines_grouped[i,0]
                lines_grouped[i,1] = lines_grouped[i,1] % theta_scaling_factor
        attempts = attempts - 1
    lines_grouped[:,1] = math.pi * lines_grouped[:,1] / theta_scaling_factor


    #
    #     Calculate Board Corners
    #

    
    corners = []

    # Categorize lines based on label determined earlier.
    lines_vertical = []
    lines_horizontal = []
    for i in range(4):
        if line_type[i] == 0:
            lines_vertical.append(lines_grouped[i])
        else:
            lines_horizontal.append(lines_grouped[i])

    # Calculate each vert/horz line pairs' intersections.
    for curr_line_vert in lines_vertical:
        for curr_line_horiz in lines_horizontal:
            corners.append(calculate_intersection(curr_line_vert, curr_line_horiz))
    
    corners = np.array(corners)

    # Test if any corner is unusually far away.
    for corner in corners:
        for coordinate in corner:
            upper_bound = max(original_image.shape[0], original_image.shape[1])
            boundaries = upper_bound * 0.3
            if coordinate > upper_bound + boundaries or coordinate < -1 * boundaries:
                return None

    # Sort corners in known order for the homography.
    corners_sorted = convex_hull_sort(corners)

    
    # 
    #     Perform homography
    #

    
    known_corners = np.array([[0, 0], [200, 0], [200, 200], [0, 200]])
    homography_matrix, _ = cv.findHomography(corners_sorted, known_corners)
    readjusted_image = cv.warpPerspective(original_image, homography_matrix, (200, 200))

    readjusted_image = np.mean(readjusted_image, axis=-1)
    readjusted_image = cv.normalize(readjusted_image, None, alpha=0, beta=1, norm_type=cv.NORM_MINMAX, dtype=cv.CV_32F)

    return readjusted_image