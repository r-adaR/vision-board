import cv2
import numpy as np
import math
from matplotlib import pyplot as plt
from matplotlib import image as im
import sklearn.cluster as cluster
from hough_transform import hough
from agglom_functions import agglom_cluster, calc_slopes
from dbscan_functions import cacl_mean_point, calc_mean_slope, calc_intersections

# Enumeration
EPSILON = 60

# Read image as grayscale.
img = cv2.imread('vision-board/BACKEND/checkboard.jpg', cv2.IMREAD_GRAYSCALE)
assert img is not None, "File not found."

print("begin")

# Perform canny edge detection.
edges = cv2.Canny(img,100,200)
print(type(edges))
cdst = cv2.cvtColor(edges, cv2.COLOR_GRAY2BGR)

# Perform hough transform
lines = hough(edges, cdst)

# Show image.
while True:
    cv2.imshow('test', cdst)
    if cv2.waitKey(1) == ord('q'):
        break

# AGGLOMERATIVE CLUSTERING PREP FUNCTIONS
line_slopes = calc_slopes(lines) # Calc slopes
slope_clusters = agglom_cluster(lines, line_slopes) # Calculate vertical/horizontal line clusters 


# DBSCAN PREP FUNCTIONS
LINE_DIRECTION = 0 # line direction = 0 
num_directional_lines = 0 # used to count number of lines that are equal to LINE_DIRECTION 

# Calculates the mean point in the image
avg_x, avg_y, num_directional_lines = cacl_mean_point(lines, slope_clusters, cdst, LINE_DIRECTION, num_directional_lines) 

# Caclulates the mean slope of all of the LINE_DIRECTION = 0 lines 
avg_slope_0 = calc_mean_slope(lines, slope_clusters, 0, line_slopes, num_directional_lines) 
# Caclulates the mean slope of all of the LINE_DIRECTION = 1 lines 
avg_slope_1 = calc_mean_slope(lines, slope_clusters, 1, line_slopes, num_directional_lines) 

print(avg_slope_0)
print(avg_slope_1)



# calculate y-intercept (b) of mean vertical line
# b = y - mx
y_intercept_avg_vert_line = avg_y - (avg_slope_0 * avg_x)
y_intercept_avg_horizontal_line = avg_y - (avg_slope_1 * avg_x)


# calculates the points of intersection between avg line (vertical & horizontal) & each actual line
intersections, intersections_and_slope = calc_intersections(cdst, slope_clusters, LINE_DIRECTION, lines, line_slopes, y_intercept_avg_vert_line,y_intercept_avg_horizontal_line, avg_slope_0, avg_slope_1)


while True:
    cv2.imshow('test', cdst)
    if cv2.waitKey(1) == ord('q'):
        break



# DBSCAN
# (FUNCTION)
intercept_clusters = cluster.DBSCAN(eps=EPSILON, min_samples=1).fit(np.array(intersections))
print(intercept_clusters.labels_)

for i in range(len(intersections)):
    if intercept_clusters.labels_[i] >= 0:
        cv2.circle(cdst, (int(intersections[i][0]), int(intersections[i][1])), 5, (int(intercept_clusters.labels_[i]*21), 0, 0), 10)

        
while True:
    cv2.imshow('test', cdst)
    if cv2.waitKey(1) == ord('q'):
        break




# (FUNCTION)
cluster_dict = dict()

for i in range(len(intercept_clusters.labels_)):
    if intercept_clusters.labels_[i] not in cluster_dict:
        cluster_dict[intercept_clusters.labels_[i]] = [intersections[i]]
    else:
        cluster_dict[intercept_clusters.labels_[i]].append(intersections[i])

# (FUNCTION) Calculate the average point and slope for each cluster.
merged_points = dict()
for c in cluster_dict:
    point_sum = [0.0, 0.0]
    
    for p in cluster_dict[c]:
        point_sum[0] += p[0]
        point_sum[1] += p[1]
    
    point_average = [(point_sum[0]/len(cluster_dict[c])), (point_sum[1]/len(cluster_dict[c]))]
    merged_points[c] = point_average


for i in merged_points:
    cv2.circle(cdst, (np.int_(merged_points[i][0]), np.int_(merged_points[i][1])), 5, (255, 244, 244), 5)

while True:
    cv2.imshow('test', cdst)
    if cv2.waitKey(1) == ord('q'):
        break