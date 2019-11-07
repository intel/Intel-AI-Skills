# Intruder Detection

## Use Case and High-Level Description

The intruder detection skill is used to protect the privacy of the video information when an intruder is detected. The intruder could be detected as an additional face or person in the video frame. The skill notifies the application if an intruder is detected. Based on that, action can be taken to protect the video information such as blurring the video. In the sample application that accompanies the skill, the camera preview is surrounded by a green frame border. If intruder is detected, the border turns red. 

## Example

![IntruderDetection sample output](doc/IntruderDetection.PNG)

## Inputs

<ol style="list-style-type: circle">
<li>Any size image with BGRA (8 bit) color format</li>
</ol>

## Outputs

<ol style="list-style-type: circle">
<li>Is intruder detected.</li>
</ol>

## API Specification

For details on Windows\* Vision Skills API, please refer to Microsoft\* <a href="https://docs.microsoft.com/en-us/windows/ai/windows-vision-skills/important-api-concepts" target="_blank">documentation</a>. Intruder detection skill implements three components:

<ol style="list-style-type: circle">
<li><a class="el" href="#intruderdetectiondescriptor">IntruderDetectionDescriptor</a></li>
<li><a class="el" href="#intruderdetectionskill">IntruderDetectionSkill</a></li>
<li><a class="el" href="#intruderdetectionbinding">IntruderDetectionBinding</a></li>
</ol>

### IntruderDetectionDescriptor

Implements Microsoft::AI::Skills::SkillInterfacePreview::ISkillDescriptor.

#### IntruderDetectionDescriptor

Constructor for intruder detection descriptor runtime class component.

```csharp
IntruderDetectionDescriptor()

Parameters: None

Returns: Microsoft::AI::Skills::SkillInterfacePreview::ISkillDescriptor as Intel::AI::Skills::IntruderDetection::IntruderDetectionDescriptor
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

Instantiates the intruder detection skill. The execution device is automatically selected by the skill.

```csharp
IAsyncOperation<ISkill>CreateSkillAsync()

Parameters: None

Returns: Microsoft::AI::Skills::SkillInterfacePreview::ISkill intruderDetectionSkill
```
&NewLine;

#### CreateSkillAsync (executionDevice)

Instantiates the intruder detection skill using the executionDevice provided by the user.

```csharp
IAsyncOperation<ISkill>CreateSkillAsync(ISkillExecutionDevice executionDevice)

Parameters: Microsoft::AI::Skills::SkillInterfacePreview::ISkillExecutionDevice executionDevice

Returns: Microsoft::AI::Skills::SkillInterfacePreview::ISkill intruderDetectionSkill
```
&NewLine;

### IntruderDetectionSkill

Implements Microsoft::AI::Skills::SkillInterfacePreview::ISkill.

#### CreateSkillBindingAsync

Instantiates the intruder detection binding object.

```csharp
IAsyncOperation<ISkillBinding> CreateSkillBindingAsync()

Parameters: None

Returns: Microsoft::AI::Skills::SkillInterfacePreview::ISkillBinding as Intel::AI::Skills::IntruderDetection::IntruderDetectionBinding
```
&NewLine;

#### EvaluateAsync

Executes the skill logic using input features provided by the binding object.

```csharp
IAsyncAction EvaluateAsync(ISkillBinding const binding)

Parameters: Intel::AI::Skills::IntruderDetection::IntruderDetectionBinding binding

Returns: Asynchronous method with no direct return value
```
&NewLine;

### IntruderDetectionBinding

Implements Microsoft::AI::Skills::SkillInterfacePreview::ISkillBinding. The intruder detection binding is created by IntruderDetectionSkill object.

#### SetInputImageAsync

Uses the VideoFrame object (frame) and binds it to the input. This is the image frame on which intruder detection is performed.

```csharp
IAsyncAction SetInputImageAsync(VideoFrame const frame)

Parameters: Windows::Media::VideoFrame frame

Returns: Asynchronous method with no direct return value
```
&NewLine;

## Application

The application developer starts with a C# XAML form based project. As a first step, s/he needs to ingest intruder detection skill by installing the nuget package from appropriate location. This makes all intruder detection skill APIs available to use. The GUI design is up to the application developer. Below pseudo code show how the skill APIs can be used to perform intruder detection.

```csharp
using Intel.AI.Skills.IntruderDetection;
using Microsoft.AI.Skills.SkillInterfacePreview;

IntruderDetectionDescriptor skillDescriptor = null;
IReadOnlyList<ISkillExecutionDevice> availableExecutionDevices = null;
IntruderDetectionSkill skill = null;
IntruderDetectionBinding binding = null;
int selectedDeviceId = 0;

// Instantiate skill descriptor
skillDescriptor = new IntruderDetectionDescriptor();

// Get list of available execution devices
availableExecutionDevices = await skillDescriptor.GetSupportedExecutionDevicesAsync();

// Initialize skill with the selected execution device
skill = await skillDescriptor.CreateSkillAsync(availableExecutionDevices[selectedDeviceId]) as IntruderDetectionSkill;

// Instantiate skill binding object
binding = await skill.CreateSkillBindingAsync() as IntruderDetectionBinding;

// Set input image frame which can be from a camera feed or a file read
await binding.SetInputImageAsync(frame);

// Run intruder detection skill with the binding object
await skill.EvaluateAsync(binding);

// Consume result
var intruderFlag = (m_paramsSkillObj.m_binding["IntruderDetected"].FeatureValue as SkillFeatureTensorFloatValue).GetAsVectorView();
bool isIntruderDetected = ((int)intruderFlag[0] != 0);

TakeAction(isIntruderDetected);
```

## Known issues

Skill currently works best in video mode. Image mode will be enabled in future releases.
&NewLine;
----

##### *Legal Information: Other names and brands may be claimed as the property of others.
