<script setup lang="ts">
import { ref, onMounted } from 'vue'
import FileUpload from '@/components/FileUpload.vue'
import ValidationResults from '@/components/ValidationResults.vue'
import ValidationOptions from '@/components/ValidationOptions.vue'
import AboutSection from '@/components/AboutSection.vue'
import { ValidationService } from '@/services/ValidationService'
import type { IValidationResponse } from '@/types/IValidationResponse'
import { TemplateService } from '@/services/TemplateService'
import type { ITemplateDto } from '@/types/ITemplateDto'
import type { IValidationOptions } from '@/types/IValidationOptions'

const templateService = new TemplateService()
const templates = ref<ITemplateDto[]>([])

onMounted(async () => {
  const response = await templateService.getTemplatesAsync()
  if (response.data) {
    templates.value = response.data
  }
})

const validationService = new ValidationService()

const selectedFile = ref<File | null>(null)
const isLoading = ref(false)
const result = ref<IValidationResponse | null>(null)
const errors = ref<string[]>([])
const options = ref<IValidationOptions>({
  templateId: 'taltech-it',
  thesisType: 'bachelor',
  language: 'et',
  curriculumLanguage: 'et',
  foreignTitle: ''
})

const onFileSelected = (file: File) => {
  selectedFile.value = file
  result.value = null
  errors.value = []
}

const onOptionsChanged = (newOptions: IValidationOptions) => {
  options.value = newOptions
}

const validate = async () => {
  if (!selectedFile.value) {
    return
  }

  isLoading.value = true
  errors.value = []
  result.value = null

  const formData = new FormData()
  formData.append('file', selectedFile.value)

  const response = await validationService.validateAsync(
    formData,
    options.value.language,
    options.value.templateId,
    options.value.thesisType,
    options.value.curriculumLanguage,
    options.value.foreignTitle
  )

  if (response.errors) {
    errors.value = response.errors
  } else if (response.data) {
    result.value = response.data
  }

  isLoading.value = false
}
</script>

<template>
  <div class="max-w-4xl mx-auto px-6 py-12 space-y-16">
    <section id="upload" class="space-y-6">
      <div>
        <h1 class="text-2xl font-semibold text-gray-900">Lõputöö valideerimine</h1>
        <p class="mt-2 text-gray-600">Lae üles oma lõputöö DOCX formaadis ja kontrolli selle vastavust struktuuri- ja
          vormistusnõuetele.</p>
      </div>

      <FileUpload @file-selected="onFileSelected" />

      <ValidationOptions :templates="templates" @options-changed="onOptionsChanged" />

      <div v-if="errors.length > 0" class="text-red-600 text-sm">
        <p v-for="error in errors" :key="error">{{ error }}</p>
      </div>

      <div v-if="selectedFile" class="flex justify-center">
        <button @click="validate" :disabled="isLoading"
          class="bg-emerald-600 text-white px-8 py-3 rounded-lg hover:bg-emerald-700 disabled:opacity-50 transition-colors font-medium text-base shadow-lg shadow-emerald-200 ring-2 ring-emerald-100 flex items-center gap-2 cursor-pointer">
          <span v-if="isLoading"
            class="w-4 h-4 border-2 border-white border-t-transparent rounded-full animate-spin"></span>
          {{ isLoading ? 'Valideerimine...' : 'Valideeri lõputöö' }}
        </button>
      </div>
    </section>

    <ValidationResults v-if="result" :result="result" />

    <AboutSection />
  </div>
</template>
