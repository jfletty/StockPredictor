import pandas
import io
import csv
import pickle
import re

def get_model_input_content(bucketName, key):
    s3_object = s3.Object(bucket_name=bucketName, key=key)

    body = s3_object.get()['Body'].read().decode('utf-8')
    df = pandas.read_csv(io.StringIO(body), quoting=csv.QUOTE_ALL)
    
    stockKey = re.sub("[^0-9]", "", key)
    return df, stockKey


def save_model(name, model):
    s3_object = s3.Object(Container.model_output, name)
    model_data = pickle.dumps(model)
    s3_object.put(Body=model_data)


def get_model(stockKey):
    s3_object = s3.Object(Container.model_output, stockKey)
    body = s3_object.get()['Body'].read()
    
    clf = pickle.loads(body)
    return clf

def get_projection_input_content(bucketName, key):
    s3_object = s3.Object(bucket_name=bucketName, key=key)

    body = s3_object.get()['Body'].read().decode('utf-8')
    df = pandas.read_csv(io.StringIO(body), quoting=csv.QUOTE_ALL)
    
    stockKey = key.split('.',1)[0]
    return df, stockKey

def save_projection(stockKey, df):
    text = df.to_csv(index=False, quoting=csv.QUOTE_NONE, quotechar='"')    
    s3_object = s3.Object(Container.projection_output, stockKey + ".csv")
    s3_object.put(Body=text)

def cleanup_inputs(container, itemName):
    s3_object = s3.Object(container, itemName)
    s3_object.delete()


class Container:
    # For the dataset to calculate a new projection model
    model_input = "daily-model-input"
    # The trained projection model
    model_output = "daily-model-output"

    projection_input = "daily-projection-input"
    # The output projection
    projection_output = "daily-projection-output"
