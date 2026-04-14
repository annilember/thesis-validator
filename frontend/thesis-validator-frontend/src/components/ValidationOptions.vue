<script setup lang="ts">
import { ref } from 'vue'

export interface ValidationOptions {
  templateId: string
  thesisType: 'bachelor' | 'master'
  language: 'et'
  curriculumLanguage: 'et' | 'en'
  foreignTitle: string
}

const emit = defineEmits<{
  optionsChanged: [options: ValidationOptions]
}>()

const options = ref<ValidationOptions>({
  templateId: 'taltech-it',
  thesisType: 'bachelor',
  language: 'et',
  curriculumLanguage: 'et',
  foreignTitle: ''
})

const onChanged = () => {
  emit('optionsChanged', { ...options.value })
}
</script>

<template>
  <div class="bg-white border border-gray-200 rounded-xl p-6 space-y-4">
    <h2 class="font-medium text-gray-900">Valideerimise seaded</h2>

    <div class="grid grid-cols-2 gap-4">
      <div class="space-y-1">
        <label class="text-sm text-gray-600">Valideerimismall</label>
        <select v-model="options.templateId" @change="onChanged"
          class="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm text-gray-900 bg-white cursor-pointer">
          <option value="taltech-it">TalTech IT-teaduskond (2025)</option>
        </select>
      </div>

      <div class="space-y-1">
        <label class="text-sm text-gray-600">Töö tüüp</label>
        <select v-model="options.thesisType" @change="onChanged"
          class="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm text-gray-900 bg-white cursor-pointer">
          <option value="bachelor">Bakalaureusetöö</option>
          <option value="master">Magistritöö</option>
        </select>
      </div>

      <div class="space-y-1">
        <label class="text-sm text-gray-600">Töö keel</label>
        <select v-model="options.language" @change="onChanged"
          class="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm text-gray-900 bg-white cursor-pointer">
          <option value="et">Eesti keel</option>
        </select>
      </div>

      <div class="space-y-1">
        <label class="text-sm text-gray-600">Õppekava keel</label>
        <select v-model="options.curriculumLanguage" @change="onChanged"
          class="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm text-gray-900 bg-white cursor-pointer">
          <option value="et">Eestikeelne õppekava</option>
        </select>
      </div>
    </div>

    <div class="space-y-1">
      <label class="text-sm text-gray-600">Võõrkeelne pealkiri</label>
      <input v-model="options.foreignTitle" @input="onChanged" type="text"
        placeholder="Sisesta töö ingliskeelne pealkiri"
        class="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm text-gray-900" />
    </div>
  </div>
</template>
