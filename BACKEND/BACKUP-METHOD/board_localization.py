import cv2
import numpy as np
import math
from matplotlib import pyplot as plt
from matplotlib import image as im
import sklearn.cluster as cluster
from hough_transform import hough
from agglom_functions import *
from dbscan_functions import *

# Enumeration
EPSILON = 60
LINE_DIRECTION = 0 # line direction = 0 

# Read image as grayscale.
img = cv2.imread('easy_0.png', cv2.IMREAD_GRAYSCALE)
assert img is not None, "File not found."
height, width = img.shape

# Perform canny edge detection.
edges = cv2.Canny(img,100,200)
cdst = cv2.cvtColor(edges, cv2.COLOR_GRAY2BGR)

# Perform hough transform
lines = hough(edges, cdst)

# Show image.
while True:
    cv2.imshow('test', cdst)
    if cv2.waitKey(1) == ord('q'):
        break

# AGGLOMERATIVE CLUSTERING PREP FUNCTIONS
line_slopes = calculate_slopes(lines) # Calc slopes
slope_clusters = agglom_cluster(lines, line_slopes) # Calculate vertical/horizontal line clusters 


# DBSCAN PREP FUNCTIONS

# Calculates the mean point in the image
avg_x, avg_y, num_directional_lines = calculate_mean_point(lines, slope_clusters, cdst, LINE_DIRECTION) 

#Calculates the mean slope of all of the LINE_DIRECTION = 0 lines 
avg_slope_0, lines_with_slopes_0 = calculate_mean_slope(lines, slope_clusters, 0, line_slopes, num_directional_lines) 
# Calculates the mean slope of all of the LINE_DIRECTION = 1 lines 
avg_slope_1, lines_with_slopes_1 = calculate_mean_slope(lines, slope_clusters, 1, line_slopes, num_directional_lines)

# calculate y-intercept (b) of mean vertical line
# b = y - mx
y_intercept_avg_vert_line = avg_y - (avg_slope_0 * avg_x)
y_intercept_avg_horizontal_line = avg_y - (avg_slope_1 * avg_x)

# calculates the points of intersection between avg line (vertical & horizontal) & each actual line
intersections, intersections_and_slope = calculate_intersections(cdst, slope_clusters, LINE_DIRECTION, lines, line_slopes, y_intercept_avg_vert_line,y_intercept_avg_horizontal_line, avg_slope_0, avg_slope_1)


while True:
    cv2.imshow('test', cdst)
    if cv2.waitKey(1) == ord('q'):
        break


# Perform DBSCAN and obtain clusters with just the intersections, not the slopes
intercept_clusters = cluster.DBSCAN(eps=EPSILON, min_samples=1).fit(np.array(intersections))

for i in range(len(intersections)):
    if intercept_clusters.labels_[i] >= 0:
        cv2.circle(cdst, (int(intersections[i][0]), int(intersections[i][1])), 5, (int(intercept_clusters.labels_[i]*21), 0, 0), 10)

while True:
    cv2.imshow('test', cdst)
    if cv2.waitKey(1) == ord('q'):
        break


# (FUNCTION) done
cluster_dict = build_cluster_dict(intercept_clusters, intersections_and_slope)

# (FUNCTION) Calculate the average point and slope for each cluster.
merged_points = calculate_cluster_averages(cluster_dict)

for c in merged_points:
    print(f"{c}: {merged_points[c]}\n\n")

for i in merged_points:
    cv2.circle(cdst, (np.int_(merged_points[i][0][0]), np.int_(merged_points[i][0][1])), 5, (255, 244, 244), 5)

for i in merged_points:
    point, slope = merged_points[i][POINT], merged_points[i][SLOPE]

    if slope == float("inf"):
        pt1 = (point[0], 0)
        pt2 = (point[0], height)
    else:
        pt1 = (0, int(point[1] - slope * point[0]))
        pt2 = (width, int(point[1] + slope * (width - point[0])))

    cv2.line(cdst, pt1, pt2, (255, 255, 255), 2)

direction1 = []
direction2 = []

for m in merged_points:
    direction1.append(m) if merged_points[m][SLOPE] < 0 else direction2.append(m)

while True:
    cv2.imshow('test', cdst)
    if cv2.waitKey(1) == ord('q'):
        break