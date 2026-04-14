<script setup lang="ts">
import { ref } from 'vue';

const emit = defineEmits<{
  fileSelected: [file: File]
}>()

const fileInput = ref<HTMLInputElement | null>(null)
const selectedFile = ref<File | null>(null)
const error = ref<string | null>(null)
const isDragging = ref(false)

const onFileChange = (event: Event) => {
  const input = event.target as HTMLInputElement
  if (!input.files || input.files.length === 0) return

  const file = input.files[0]
  if (!file) {
    error.value = 'Faili üleslaadimine ebaõnnestus'
    selectedFile.value = null
    return
  }

  if (!file.name.endsWith('.docx')) {
    error.value = 'Palun laadige üles DOCX formaadis fail'
    selectedFile.value = null
    return
  }

  error.value = null
  selectedFile.value = file
  emit('fileSelected', file)
}

const onClick = () => fileInput.value?.click()

const onDrop = (event: DragEvent) => {
  isDragging.value = false
  const file = event.dataTransfer?.files[0]
  if (!file) return

  if (!file.name.endsWith('.docx')) {
    error.value = 'Palun laadige üles DOCX formaadis fail'
    return
  }

  error.value = null
  selectedFile.value = file
  emit('fileSelected', file)
}
</script>

<template>
  <div class="space-y-3">
    <div class="relative border-2 border-dashed rounded-xl p-10 text-center cursor-pointer transition-all duration-200"
      :class="isDragging
        ? 'border-blue-400 bg-blue-50'
        : selectedFile
          ? 'border-green-300 bg-green-50'
          : 'border-gray-200 bg-white hover:border-blue-300 hover:bg-blue-50'" @click="onClick"
      @dragover.prevent="isDragging = true" @dragleave="isDragging = false" @drop.prevent="onDrop">
      <input ref="fileInput" type="file" accept=".docx" class="hidden" @change="onFileChange" />

      <div v-if="!selectedFile" class="space-y-3">
        <div class="w-12 h-12 mx-auto rounded-full bg-blue-100 flex items-center justify-center">
          <svg class="w-6 h-6 text-blue-500" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2"
              d="M7 16a4 4 0 01-.88-7.903A5 5 0 1115.9 6L16 6a5 5 0 011 9.9M15 13l-3-3m0 0l-3 3m3-3v12" />
          </svg>
        </div>
        <div>
          <p class="text-gray-700 font-medium">Lohista fail siia või klõpsa valimiseks</p>
          <p class="text-sm text-gray-400 mt-1">Toetatud formaat: DOCX</p>
        </div>
      </div>

      <div v-else class="space-y-2">
        <div class="w-12 h-12 mx-auto rounded-full bg-green-100 flex items-center justify-center">
          <svg class="w-6 h-6 text-green-500" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2"
              d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z" />
          </svg>
        </div>
        <p class="font-medium text-gray-900">{{ selectedFile.name }}</p>
        <p class="text-sm text-gray-500">{{ (selectedFile.size / 1024).toFixed(1) }} KB</p>
        <p class="text-xs text-gray-400 mt-2">Muutmiseks klõpsa või lohista uus fail</p>
      </div>
    </div>

    <p v-if="error" class="text-sm text-red-600 flex items-center gap-1">
      <svg class="w-4 h-4" fill="currentColor" viewBox="0 0 20 20">
        <path fill-rule="evenodd"
          d="M18 10a8 8 0 11-16 0 8 8 0 0116 0zm-7 4a1 1 0 11-2 0 1 1 0 012 0zm-1-9a1 1 0 00-1 1v4a1 1 0 102 0V6a1 1 0 00-1-1z"
          clip-rule="evenodd" />
      </svg>
      {{ error }}
    </p>
  </div>
</template>
