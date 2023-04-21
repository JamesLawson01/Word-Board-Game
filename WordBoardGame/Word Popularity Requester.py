import requests
import time

previousLetter = ''

def request(url):
    try:
        r = requests.get(url)
        return r.json()
    except:
        print(word)
        print("code: " + str(r.status_code))
        print(r.text)
        time.sleep(20)
        return request(url)


#clear file before starting
open("WordBoardGame/WordBoardGame/word popularities.txt", "w").close()

with open("WordBoardGame/WordBoardGame/Sowpods.txt") as file:
    words = file.readline().replace("\n", "")

    for word in file.read().splitlines()[1:]:
        words += "%2C" + word

    url = f"https://books.google.com/ngrams/json?content={words}&year_start=2018&year_end=2019&corpus=26&smoothing=0"

    r = requests.get(url).json()



    '''if word[0] != previousLetter:
            previousLetter = word[0]
            print(previousLetter)

        url = f"https://books.google.com/ngrams/json?content={word}&year_start=2018&year_end=2019&corpus=26&smoothing=0"
        try:
            response = request(url)
        except:
            time.sleep(2)
            print(word)
            response = request(url)
            raise 
        popularity = 0

        if len(response) > 1:
            print(response)
            raise Exception(f"{word} gave an unexpected json format")

        elif len(response) == 1:
            popularities = response[0]["timeseries"]

            if len(popularities) == 2:
                popularity = popularities[1]

            else:
                print(response)
                raise Exception(f"{word} gave unexpected number of popularites")

        with open("WordBoardGame/WordBoardGame/word popularities.txt", "a") as outputFile:
            outputFile.write(word + "\t" + str(popularity))'''

