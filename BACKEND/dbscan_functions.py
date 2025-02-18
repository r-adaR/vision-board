import math
import cv2


def cacl_mean_point(lines, slope_clusters, cdst, LINE_DIRECTION, num_directional_lines):
    sum_x = 0
    sum_y = 0
    
    

    # track max and min x value and/or y value

    for i in range(len(lines)):
        # only counts vertical or horizontal lines (depending on set to 0 or 1)
        if slope_clusters[i] == LINE_DIRECTION:
            cv2.line(cdst, (lines[i][0][0], lines[i][0][1]), (lines[i][0][2], lines[i][0][3]), (255,0,0), 3, cv2.LINE_AA)
            sum_x += lines[i][0][0] + lines[i][0][2]
            sum_y += lines[i][0][1] + lines[i][0][3]
            num_directional_lines += 1

    avg_x = int(sum_x / (2 * num_directional_lines))
    avg_y = int(sum_y / (2 * num_directional_lines))
    return avg_x, avg_y, num_directional_lines


def calc_mean_slope(lines, slope_clusters, LINE_DIRECTION, line_slopes, num_directional_lines):
    avg_angle = 0
    for i in range(len(lines)):
        # only counts vertical or horizontal lines (depending on set to 0 or 1)
        if slope_clusters[i] == LINE_DIRECTION:
            avg_angle += math.atan(line_slopes[i])



    # avg_slope = avg_slope / (2 * num_directional_lines)
    print(num_directional_lines)
    avg_slope = math.tan(avg_angle / (num_directional_lines))
    return avg_slope



def calc_intersections(cdst, slope_clusters, LINE_DIRECTION, lines, line_slopes, y_intercept_avg_vert_line,y_intercept_avg_horizontal_line, avg_slope_0, avg_slope_1):
    intersections = []
    # need to change this to 
    intersections_and_slope = []


    for i in range(len(lines)):
        # only counts vertical or horizontal lines (depending on set to 0 or 1)
        if slope_clusters[i] != LINE_DIRECTION:
            # b = y - mx
            y_intercept_horizontal_line = lines[i][0][1] - (line_slopes[i] * lines[i][0][0])
            intersection_x = (y_intercept_avg_vert_line - y_intercept_horizontal_line) / (line_slopes[i] - avg_slope_0)
            intersection_y = ((avg_slope_0 * intersection_x) + y_intercept_avg_vert_line)
            intersections.append([intersection_x, intersection_y])
            intersections_and_slope.append(([intersection_x, intersection_y], line_slopes[i]))
            cv2.circle(cdst, (int(intersection_x), int(intersection_y)), 5, (0, 255, 0), 10)



        else:
            y_intercept_horizontal_line = lines[i][0][1] - (line_slopes[i] * lines[i][0][0])
            intersection_x = (y_intercept_avg_horizontal_line - y_intercept_horizontal_line) / (line_slopes[i] - avg_slope_1)
            intersection_y = ((avg_slope_1 * intersection_x) + y_intercept_avg_horizontal_line)
            intersections.append([intersection_x, intersection_y])
            intersections_and_slope.append(([intersection_x, intersection_y], line_slopes[i]))
            cv2.circle(cdst, (int(intersection_x), int(intersection_y)), 5, (0, 255, 0), 10)



    return intersections, intersections_and_slope

