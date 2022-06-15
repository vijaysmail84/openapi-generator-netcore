# OpenAPI Generator .Net 6

## Console application to generate WebAPI project for .Net 6 

### Console Input options 


```bash
BuilderType = "netcore" 
Version = "6.0"

UseSampelInput = false (If no input is provided used pet store YAML)
OpeAPISpecPath = @"C:\api.yaml";

OutputPath = @"C:\src02";
SolutionName = "New.Service";
ProjectName = "Pets.API";
BreakProjectsByTags = true;
```

### Sample YAML
```bash
openapi: "3.0.0"
info:
  version: 1.0.0
  title: Swagger Petstore
  license:
    name: MIT
servers:
  - url: http://petstore.swagger.io/v1
paths:
  /pets:
    get:
      summary: List all pets
      operationId: listPets
      tags:
        - pets
      parameters:
        - name: limit
          in: query
          description: How many items to return at one time (max 100)
          required: false
          schema:
            type: integer
            format: int32
      responses:
        '200':
          description: A paged array of pets
          headers:
            x-next:
              description: A link to the next page of responses
              schema:
                type: string
          content:
            application/json:    
              schema:
                $ref: "#/components/schemas/Pets"
        "500":
          description: Unexpected serve error
          content:
            application/json:
              schema:
                $ref: "#/components/schemas/Error"
    post:
      summary: Create a pet
      operationId: createPets
      tags:
        - cats
      responses:
        '201':
          description: Null response
        '200':
          description: Response for 200 test
          headers:
            x-next:
              description: A link to the next page of responses
              schema:
                type: string
          content:
            application/json:    
              schema:
                $ref: "#/components/schemas/TestResponse"
      requestBody:
        description: Pet request
        content:
          application/json:
            schema:
              $ref: '#/components/schemas/TestRequest'
        default:
          description: unexpected error
          content:
            application/json:
              schema:
                $ref: "#/components/schemas/Error"
  /pets/{petId}:
    get:
      summary: Info for a specific pet
      operationId: showPetById
      tags:
        - pets
      parameters:
        - name: petId
          in: path
          required: true
          description: The id of the pet to retrieve
          schema:
            type: string
      responses:
        '200':
          description: Expected response to a valid request
          content:
            application/json:
              schema:
                $ref: "#/components/schemas/Pet"
        default:
          description: unexpected error
          content:
            application/json:
              schema:
                $ref: "#/components/schemas/Error"
components:
  schemas:
    Pet:
      type: object
      required:
        - id
        - name
      properties:
        id:
          type: integer
          format: int64
        name:
          type: string
        tag:
          type: string
        context:
           $ref: "#/components/schemas/Context"
    Pets:
      type: array
      items:
        $ref: "#/components/schemas/Pet"
    Error:
      type: object
      description: "Common Error Response."
      required:
        - code
        - message
      properties:
        code:
          type: integer
          format: int32
          description: "Error code."
        message:
          type: string
          description: "Error message."
    Context:
      type: object
      properties:
        tenant:
          type: integer
          format: int64
        somekey:
          type: string
          format: string
        commoncontext:
           $ref: "#/components/schemas/CommonTenant"
    TestResponse:
      type: object
      properties:
        petage:
          type: string
          format: string
    TestRequest:
      type: object
      properties:
        petname:
          type: string
          format: string
        pettype:
          type: string
          format: string
        commontenant:
           $ref: "#/components/schemas/CommonTenant"
    CommonTenant:
      type: object
      properties:
        commontenentprop:
          type: string
          format: string
       
 
```

### Output project structure

