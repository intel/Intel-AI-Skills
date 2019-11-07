# Person Detection

## Use Case and High-Level Description

The person detection skill is used to detect multiple people in video or image inputs. It uses OpenVINO™ based AI inference to output bounding boxes or rectangles corresponding to the persons detected by the algorithm. In addition, a confidence score for each detected person is also available as output.

## Example

![PersonDetection sample output](doc/PersonDetection.PNG)

## Inputs

<ol style="list-style-type: circle">
<li>Any size image with BGRA (8 bit) color format</li>
</ol>

## Outputs

<ol style="list-style-type: circle">
<li>Number of persons detected.</li>
<li>Person rectangle(s) co-ordinates normalized with respect to input image dimensions {Left.x, Top.y, Right.x, Bottom.y}, range [0, 1.0].</li>
<li>Confidence score for each person detection, range [0, 1.0].</li>
</ol>

## API Specification

For details on Windows\* Vision Skills API, please refer to Microsoft\* <a href="https://docs.microsoft.com/en-us/windows/ai/windows-vision-skills/important-api-concepts" target="_blank">documentation</a>. Person detection skill implements three components:

<ol style="list-style-type: circle">
<li><a class="el" href="#persondetectiondescriptor">PersonDetectionDescriptor</a></li>
<li><a class="el" href="#persondetectionskill">PersonDetectionSkill</a></li>
<li><a class="el" href="#persondetectionbinding">PersonDetectionBinding</a></li>
</ol>

### PersonDetectionDescriptor

Implements Microsoft::AI::Skills::SkillInterfacePreview::ISkillDescriptor.

#### PersonDetectionDescriptor

Constructor for person detection descriptor runtime class component.

```csharp
PersonDetectionDescriptor()

Parameters: None

Returns: Microsoft::AI::Skills::SkillInterfacePreview::ISkillDescriptor as Intel::AI::Skills::PersonDetection::PersonDetectionDescriptor
```
&NewLine;

#### Information

Returns structure containing information about the skill which includes name, description, author, publisher and version.

```csharp
SkillInformation Information()

Parameters: None

Returns: SkillInformation information
```
&NewLine;

#### InputFeatureDescriptors

Returns the set of input features used by the skill.

```csharp
IVectorView<ISkillFeatureDescriptor> InputFeatureDescriptors()

Parameters: None

Returns: IVectorView<Microsoft::AI::Skills::SkillInterfacePreview::ISkillFeatureDescriptor> inputFeatures
```
&NewLine;

#### OutputFeatureDescriptors

Returns the set of output features provided by the skill.

```csharp
IVectorView<ISkillFeatureDescriptor> OutputFeatureDescriptors()

Parameters: None

Returns: IVectorView<Microsoft::AI::Skills::SkillInterfacePreview::ISkillFeatureDescriptor> outputFeatures
```
&NewLine;

#### GetSupportedExecutionDevicesAsync

Returns a list of devices of hardware accelerators available on the platform which can be used to execute the skill logic.

```csharp
IAsyncOperation<IVectorView<ISkillExecutionDevice>> GetSupportedExecutionDevicesAsync()

Parameters: None

Returns: IVectorView<Microsoft::AI::Skills::SkillInterfacePreview::ISkillExecutionDevice> outputFeatures
```
&NewLine;

#### CreateSkillAsync

Instantiates the person detection skill. The execution device is automatically selected by the skill.

```csharp
IAsyncOperation<ISkill>CreateSkillAsync()

Parameters: None

Returns: Microsoft::AI::Skills::SkillInterfacePreview::ISkill personDetectionSkill
```
&NewLine;

#### CreateSkillAsync (executionDevice)

Instantiates the person detection skill using the executionDevice provided by the user.

```csharp
IAsyncOperation<ISkill>CreateSkillAsync(ISkillExecutionDevice executionDevice)

Parameters: Microsoft::AI::Skills::SkillInterfacePreview::ISkillExecutionDevice executionDevice

Returns: Microsoft::AI::Skills::SkillInterfacePreview::ISkill personDetectionSkill
```
&NewLine;

### PersonDetectionSkill

Implements Microsoft::AI::Skills::SkillInterfacePreview::ISkill.

#### CreateSkillBindingAsync

Instantiates the person detection binding object.

```csharp
IAsyncOperation<ISkillBinding> CreateSkillBindingAsync()

Parameters: None

Returns: Microsoft::AI::Skills::SkillInterfacePreview::ISkillBinding as Intel::AI::Skills::PersonDetection::PersonDetectionBinding
```
&NewLine;

#### EvaluateAsync

Executes the skill logic using input features provided by the binding object.

```csharp
IAsyncAction EvaluateAsync(ISkillBinding const binding)

Parameters: Intel::AI::Skills::PersonDetection::PersonDetectionBinding binding

Returns: Asynchronous method with no direct return value
```
&NewLine;

### PersonDetectionBinding

Implements Microsoft::AI::Skills::SkillInterfacePreview::ISkillBinding. The person detection binding is created by PersonDetectionSkill object.

#### SetInputImageAsync

Uses the VideoFrame object (frame) and binds it to the input. This is the image frame on which person detection is performed.

```csharp
IAsyncAction SetInputImageAsync(VideoFrame const frame)

Parameters: Windows::Media::VideoFrame frame

Returns: Asynchronous method with no direct return value
```
&NewLine;

#### PersonBB

Returns bounding box or rectangle for the person at the given index.

```csharp
IVectorView<float> PersonBB(int32_t index)

Parameters: int32_t index to specify which person rectangle from the list of identified persons to get

Returns: A 4-element float vector with person rectangle(s) co-ordinates normalized with respect to input image dimensions {Left.x, Top.y, Right.x, Bottom.y}, range [0, 1.0].
```
&NewLine;

## Application

The application developer starts with a C# XAML form based project. As a first step, s/he needs to ingest person detection skill by installing the nuget package from appropriate location. This makes all person detection skill APIs available to use. The GUI design is up to the application developer. Below pseudo code show how the skill APIs can be used to perform person detection.

```csharp
using Intel.AI.Skills.PersonDetection;
using Microsoft.AI.Skills.SkillInterfacePreview;

PersonDetectionDescriptor skillDescriptor = null;
IReadOnlyList<ISkillExecutionDevice> availableExecutionDevices = null;
PersonDetectionSkill skill = null;
PersonDetectionBinding binding = null;
int selectedDeviceId = 0;

// Instantiate skill descriptor
skillDescriptor = new PersonDetectionDescriptor();

// Get list of available execution devices
availableExecutionDevices = await skillDescriptor.GetSupportedExecutionDevicesAsync();

// Initialize skill with the selected execution device
skill = await skillDescriptor.CreateSkillAsync(availableExecutionDevices[selectedDeviceId]) as PersonDetectionSkill;

// Instantiate skill binding object
binding = await skill.CreateSkillBindingAsync() as PersonDetectionBinding;

// Set input image frame which can be from a camera feed or a file read
await binding.SetInputImageAsync(frame);

// Run person detection skill with the binding object
await skill.EvaluateAsync(binding);

// Consume result
var numP = (m_paramsSkillObj.m_binding["NumberOfPersons"].FeatureValue as SkillFeatureTensorFloatValue).GetAsVectorView();

// Number of persons is the first element of numP vector
int numPersons = (int)numP[0];

// Get probability of detection
var prob = (m_paramsSkillObj.m_binding["PersonConfidence"].FeatureValue as SkillFeatureTensorFloatValue).GetAsVectorView();

if(numPersons != 0)
{
    for (int i = 0; i < numPersons; i++)
    {
        // Display person rectangle using application's implementation of a renderer
        ShowRectangle(binding.PersonBB(i));
    }
}
```

## Known issues

Skill initialization time is high, especially for GPU mode. This is expected to be fixed in future versions. 

----

##### *Legal Information: Other names and brands may be claimed as the property of others.
