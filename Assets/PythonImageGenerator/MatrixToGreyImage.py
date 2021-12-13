from posixpath import basename
import numpy as np
import glob
import os
from PIL import Image


from enum import Enum

class Tile(Enum):
    Cloister = 1 * 25
    Village = 75
    Road = 125
    Grass = 175
    City = 250
    Stream = 6 * 25
    CityStream = 7 * 25 
    RoadStream = 8 * 25
    CityRoad = 9 * 25


filePath = glob.glob(os.path.dirname(__file__) +"/TxtFiles/*.txt")
for x in filePath:
    if os.path.isfile(os.path.dirname(__file__)+"/Images/"+os.path.splitext(x)[0]+".png"):
        continue
    else:
        with open(x,'r+') as f:
            l = []
            for line in f:
                b =[]
                for z in line.split(','):
                    z = z.rstrip("\n")
                    if z == "0":
                        b.append(int(z))
                    else:
                        b.append(int(Tile[z]._value_))
                l.append(b)
    

    input = np.asarray(l)

    #input[60,60] = 10
    #input[60,61] = 10
    #input[60,62] = 10
    #input[60,63] = 10
    #input[60,64] = 10
    #input[60,65] = 10

    #input[61,60] = 10
    #input[61,61] = 10
    #input[61,62] = 10
    #input[61,63] = 10
    #input[61,64] = 10
    #input[61,65] = 10

    #input[62,60] = 10
    #input[62,61] = 10
    #input[62,62] = 10
    #input[62,63] = 10
    #input[62,64] = 10
    #input[62,65] = 10
    

    #plt.imshow(input)
    #plt.axis("off")
    #plt.gray()
    ##print("Images\\"+os.path.splitext(x)[0]+".png")
    #plt.xlim(120)
    #plt.ylim(120)
    #plt.tight_layout()
    #plt.savefig("Images\\"+os.path.splitext(x)[0]+".png")
    img = Image.fromarray(np.uint8(input), 'L')
    print(os.path.splitext(os.path.basename(x))[0]+".png")
    img.save(os.path.dirname(__file__)+"/Images/"+os.path.splitext(os.path.basename(x))[0]+".png")
    #img.show()
        #l = [ [Tile[x.rstrip('\n')] for x in line.split(',')] for line in f]

#print(l)

#print(input)


    
#input = np.loadtxt(x,dtype="i", delimiter=",")
#print(input)


