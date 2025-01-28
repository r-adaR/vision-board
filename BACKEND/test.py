import cv2
import numpy as np

image = np.zeros((512, 512, 3), np.uint8)

cv2.line(image, (0, 0), (511, 511), (0, 255, 0), 5)

cv2.rectangle(image, (384, 0), (510, 128), (0, 0, 255), 3)

cv2.imshow('Image', image)

cv2.waitKey(0)
cv2.destroyAllWindows()



# Open Camera
cap = cv2.VideoCapture(0)

while True:
    # Read a frame from the camera
    ret, frame = cap.read()

    # Display the frame in a window
    cv2.imshow('Camera', frame)

    # Exit if the user presses 'q'
    if cv2.waitKey(1) == ord('q'):
        break

# Release the camera and close the window
cap.release()
cv2.destroyAllWindows()



